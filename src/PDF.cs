using System;
using System.Collections.Generic;

namespace IT.Services.PDF
{
    public enum Method
    {
        UNKNOWN = 0,
        APPLICATION = 1,
        EXTENSION = 2
    }


    public class PageSize
    {
        const string A5 = "A5";
        const string A4 = "A4";
        const string A3 = "A3";
        const string A2 = "A2";
        const string A1 = "A1";
        const string A0 = "A0";
    };


     public class Orientation
    {
        const string PORTRAIT = "portrait";
        const string LANDSCAPE = "landscape";
    };


    public class PrintPDF
    {
        static private Method pdfMethod = Method.UNKNOWN;
        static private string pdfapp = "/usr/bin/wkhtmltopdf";
        static private string command = "";
        private string pdffilename = "";
        private string orientation = "Portrait";
        private string pageSize = "A4";
        //private string colorMode = "Color";
        private Dictionary<string, float> margin = new Dictionary<string, float>();
        private Dictionary<string, float> spacing = new Dictionary<string, float>();

        private bool _Internal_TryFile(string filename)
        {
            if (System.IO.File.Exists(filename))
            {
                pdfapp = filename;
                pdfMethod = Method.APPLICATION;
                return true;
            }
            return false;
        }

        public string GetPDFFilePath()
        {
            return this.pdffilename;
        }

        public string GetPDFFilename()
        {
            return System.IO.Path.GetFileNameWithoutExtension(this.pdffilename);
        }

        private string _Internal_Guid_V4()
        {
            return Guid.NewGuid().ToString();
        }

        private string _Internal_CreateTempFilename(string extension = "", string path = null)
        {
            string tmpFilename;
            if (path == null)
            {
                tmpFilename = System.IO.Path.GetTempPath() + '\\' + this._Internal_Guid_V4() + extension;
                //tmpFilename = tempnam(sys_get_temp_dir(), ""). extension;
            }

            else
            {
                tmpFilename = path + extension;
            }
            return tmpFilename;
        }


        /***********************************************
         * @throws Exception
         ***********************************************/
        public PrintPDF()
        {
            margin["top"] = 12.5f;
            margin["bottom"] = 7.5f;
            margin["left"] = 15f;
            margin["right"] = 15f;

            spacing["header"] = 0f;
            spacing["footer"] = 0f;

            if (PrintPDF.pdfMethod == Method.UNKNOWN)
            {
                bool check = this._Internal_TryFile("C:\\inetpub\\wkhtmltopdf\\wkhtmltopdf.exe") ||
                this._Internal_TryFile(System.Environment.GetEnvironmentVariable("ProgramFiles") + "\\wkhtmltopdf\\wkhtmltopdf.exe") ||
                this._Internal_TryFile(System.Environment.GetEnvironmentVariable("ProgramFiles(x86)") + "\\wkhtmltopdf\\wkhtmltopdf.exe") ||
                this._Internal_TryFile(System.Environment.GetEnvironmentVariable("ProgramFiles(x86)") + "\\wkhtmltopdf-386\\wkhtmltopdf-i386.exe");

                if (!check)
                {
                    throw new Exception("Please modify the path to find wkhtmltopdf");
                }
                PrintPDF.command = PrintPDF.pdfapp + " --print-media-type --enable-smart-shrinking --zoom 1.324 ";
            }
            else
            {
                PrintPDF.pdfapp = "";
                if (this._Internal_TryFile("/usr/bin/wkhtmltopdf-i386") ||
                    this._Internal_TryFile("/usr/bin/wkhtmltopdf"))
                {
                    PrintPDF.pdfMethod = Method.APPLICATION;
                    PrintPDF.command = PrintPDF.pdfapp + " --print-media-type --enable-smart-shrinking --load-error-handling ignore ";
                }
                else
                {
                    PrintPDF.pdfMethod = Method.UNKNOWN;
                    //Δεν βρέθηκε το wkhtmltopdf, οπότε δεν μπορούμε να κάνουμε εκτύπωση σε pdf.
                    throw new Exception("Δεν βρέθηκε το wkhtmltopdf.");
                }
            }
        }

        public float GetSpacing(string where)
        {
            if (!this.spacing.ContainsKey(where))
            {
                throw new Exception("where not found.");
            }
            return (float)this.spacing[where];
        }

        public void SetSpacing(string where, float value)
        {
            if (!this.spacing.ContainsKey(where))
            {
                throw new Exception(where + " not found.");
            }
            this.spacing[where] = value;
        }

        public void SetOrientation(string orientation)
        {
            this.orientation = orientation;
        }

        public float GetMargin(string side)
        {
            if (!this.margin.ContainsKey(side))
            {
                throw new Exception("side not found.");
            }
            return (float)this.margin[side];
        }

        public void SetMargin(string side, float value)
        { }

        public object GetMargins()
        {
            return this.margin;
        }

        public void SetMargins(float top, float bottom, float left, float right)
        {
            this.margin["top"] = top;
            this.margin["bottom"] = bottom;
            this.margin["left"] = left;
            this.margin["right"] = right;
        }

        public void SetPageSize(string pageSize)
        {
            this.pageSize = pageSize;
        }

        /***********************************************
         * @throws Exception
         ***********************************************/
        public void CreateFromFile(string filename)
        {
            System.Diagnostics.Debug.Assert(System.IO.File.Exists(filename), "Το αρχείο " + filename + " δεν βρέθηκε.");
            if (!System.IO.File.Exists(filename))
            {
                throw new Exception("Το αρχείο " + filename + " δεν βρέθηκε.");
            }

            this.CreateFromURL(filename, filename);
        }


        public bool CreateFromURL(string url, string path = null)
        {
            this.pdffilename = this._Internal_CreateTempFilename(".pdf", path);

            //Αν υπάρχει το αρχείο διέγραψέ το.
            //Υπό φυσιολογικές συνθήκες αυτό δεν πρέπει να συμβεί.
            if (System.IO.File.Exists(this.pdffilename))
            {
                //assert(false, "Το αρχείο {"+this.pdffilename} δεν θα έπρεπε να υπάρχει (για να το διαγράψουμε)");
                System.IO.File.Delete(this.pdffilename);
            }

            if (PrintPDF.pdfMethod == Method.APPLICATION)
            {
                System.Diagnostics.Process.Start(
                    PrintPDF.command,
                    " --margin-top " + this.margin["top"] +
                    " --margin-left " + this.margin["left"] +
                    " --margin-right " + this.margin["right"] +
                    " --margin-bottom " + this.margin["bottom"] +
                    " --header-spacing " + this.spacing["header"] +
                    " --footer-spacing " + this.spacing["footer"] +
                    " -O " + this.orientation +
                    " -s " + this.pageSize + " " +
                    url + " " + this.pdffilename);
            }

            return (System.IO.File.Exists(this.pdffilename));
        }

        /***********************************************
         * @throws Exception
         ***********************************************/
        public void CreateFromString(string data)
        {
            string tmpfilename = this._Internal_CreateTempFilename(".html");
            System.IO.File.WriteAllText(tmpfilename, data);
            if (false == System.IO.File.Exists(tmpfilename))
            {
                throw new Exception("CreateFromString: Απέτυχε η δημιουργία του PDF.");
            }
            this.CreateFromFile(tmpfilename);
            System.IO.File.Delete(tmpfilename);
        }

        /***********************************************
         * @throws Exception
         ***********************************************/
        public void DownloadAs(string filename)
        {
            this._Internal_ShowPDF(filename, "attachment");
        }

        /***********************************************
         * @throws Exception
         ***********************************************/
        public void Display(string filename)
        {
            this._Internal_ShowPDF(filename, "inline");
        }

        /***********************************************
         * @throws Exception
         ***********************************************/
        private bool _Internal_ShowPDF(string filename, string disposition)
        {
            //Αν υπάρχει το αρχείο διέγραψέ το.
            //Υπό φυσιολογικές συνθήκες αυτό δεν πρέπει να συμβεί.
            if (!System.IO.File.Exists(this.pdffilename))
            {
                System.Diagnostics.Debug.Assert(false, "Το αρχείο " + this.pdffilename + " δεν υπάρχει");
                throw new Exception("Το αρχείο " + this.pdffilename + " δεν υπάρχει");
            }

            //header("Content-type: application/pdf");
            //header("Content-Disposition: " + disposition + ";filename=\"" + filename + "\"");
            //readfile(this.pdffilename);
            return true;
        }
    }
}
