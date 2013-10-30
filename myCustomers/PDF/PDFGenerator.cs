using System.IO;
using System;
using System.Web;

namespace myCustomers.Pdf
{
	public abstract class PDFGenerator
	{
		
		public void SavePdf( string filename )
		{
			using( FileStream fs = new FileStream(filename,FileMode.Create) )
			{
				SavePdf(fs);
			}
		}
		
		public void SavePdf( HttpResponse response, string downloadfilename )
		{
			response.Clear();
			response.ContentType = "application/x-pdf";
			response.AddHeader("content-disposition", string.Format("attachment; filename={0}",downloadfilename));
				
			SavePdf(response.OutputStream);
		}

	    public abstract void SavePdf( Stream s );
	}
}
