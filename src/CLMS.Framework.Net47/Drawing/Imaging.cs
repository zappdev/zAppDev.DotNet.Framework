using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Web.UI;

namespace CLMS.Framework.Drawing
{
	public class Imaging
	{
		public static string GetImageSource(object imagedata, Page page)
		{
			return GetImageSource(imagedata, string.Empty, page);
		}

		public static string GetImageSource(object imagedata, string imagename, Page page)
		{
			if (imagedata == null)
				return "";
		    Image dbImage = null;
		    MemoryStream imagestream = null;
		    try
		    {


		        byte[] data = (byte[]) imagedata;

		        // Save large image
		        //string filepath = "~/Temp/Images/" + Path.GetRandomFileName();
		        string filepath = "~/Temp/" + System.Web.HttpContext.Current.Session.SessionID + "/Images";
		        if (!Directory.Exists(page.Server.MapPath(filepath)))
		            Directory.CreateDirectory(page.Server.MapPath(filepath));

		        string tmpfilename = imagename;
		        //Image dbImage;
		        imagestream = new MemoryStream(data);
		        try
		        {
		            dbImage = Image.FromStream(imagestream);
		        }
		        catch (ArgumentException)
		        {
		            // Try to see if the photo is saved as an OLE object (Legacy dbs)
		            // 512 make sure we will get the whole header
		            int headerlength = Math.Min(512, data.Length);
		            byte[] tmpBuffer = new byte[headerlength];
		            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
		            imagestream.Read(tmpBuffer, 0, headerlength);
		            string header = enc.GetString(tmpBuffer);

		            // hack's out the old header info
		            if (header.Contains("PBrush"))
		            {
		                int offset = header.IndexOf("BM");
		                imagestream = new MemoryStream();
		                imagestream.Write(data, offset, data.Length - offset);
		            }

		            imagestream.Position = 0;
		            dbImage = Image.FromStream(imagestream);
		        }

		        if (string.IsNullOrEmpty(tmpfilename))
		        {
		            tmpfilename = Path.GetTempFileName();
		            tmpfilename = tmpfilename.Substring(tmpfilename.LastIndexOf("\\") + 1);
		            string extension = GetImageFilenameExtension(dbImage);
		            tmpfilename += "." + (string.IsNullOrEmpty(extension) ? "jpg" : extension);
		        }

		        string filefullpath = filepath + "/" + tmpfilename;

		        using (FileStream strm = new FileStream(page.Server.MapPath(filefullpath), FileMode.Create))
		        {
		            byte[] byteArrToWrite = imagestream.ToArray();
		            strm.Write(byteArrToWrite, 0, byteArrToWrite.Length);
		            strm.Flush();
		            strm.Close();
		        }

		        return page.ResolveClientUrl(filefullpath);
		    }
		    finally
		    {

                dbImage?.Dispose();
                imagestream?.Dispose();
            }
		}

		public static string GetThumbnailImageSource(object imagedata, Page page)
		{
			return GetThumbnailImageSource(imagedata, string.Empty, page);
		}

		public static string GetThumbnailImageSource(object imagedata, string imagename, Page page)
		{
			if (imagedata == null)
				return "";
		    Image dbImage = null;
		    Image thumbnailImage = null;
            MemoryStream imagestream = null;
		    try
		    {



		        byte[] data = (byte[]) imagedata;

		        //Image dbImage;
		        //MemoryStream imagestream = new MemoryStream(data);
		        imagestream = new MemoryStream(data);
		        try
		        {
		            dbImage = Image.FromStream(imagestream);
		        }
		        catch (ArgumentException)
		        {
		            // Try to see if the photo is saved as an OLE object (Legacy dbs)
		            // 512 make sure we will get the whole header
		            int headerlength = Math.Min(512, data.Length);
		            byte[] tmpBuffer = new byte[headerlength];
		            System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding();
		            imagestream.Read(tmpBuffer, 0, headerlength);
		            string header = enc.GetString(tmpBuffer);

		            // hack's out the old header info
		            if (header.Contains("PBrush"))
		            {
		                int offset = header.IndexOf("BM");
		                imagestream = new MemoryStream();
		                imagestream.Write(data, offset, data.Length - offset);
		            }

		            imagestream.Position = 0;
		            dbImage = Image.FromStream(imagestream);
		        }

		        const int width = 100;
		        int height = Convert.ToInt32(dbImage.Height*(Convert.ToDecimal(width)/Convert.ToDecimal(dbImage.Width)));

		        //Image thumbnailImage;

		        if (dbImage.Width < width && dbImage.Height < height)
		        {
		            thumbnailImage = dbImage;
		        }
		        else
		        {
		            thumbnailImage = dbImage.GetThumbnailImage(width, height, null, new IntPtr());
		        }

		        imagestream = new MemoryStream();
		        thumbnailImage.Save(imagestream, ImageFormat.Jpeg);

		        // Save Thumbnail image
		        //string filepath = "~/Temp/Images/" + Path.GetRandomFileName();
		        string filepath = "~/Temp/" + System.Web.HttpContext.Current.Session.SessionID + "/Images";
		        if (!Directory.Exists(page.Server.MapPath(filepath)))
		            Directory.CreateDirectory(page.Server.MapPath(filepath));

		        string tmpfilename = imagename;
		        if (string.IsNullOrEmpty(tmpfilename))
		        {
		            tmpfilename = Path.GetTempFileName();
		            tmpfilename = tmpfilename.Substring(tmpfilename.LastIndexOf("\\") + 1);
		            string extension = GetImageFilenameExtension(thumbnailImage);
		            tmpfilename += "." + (string.IsNullOrEmpty(extension) ? "jpg" : extension);
		        }

		        string filefullpath = filepath + "/tbn_" + tmpfilename;

		        using (FileStream strm = new FileStream(page.Server.MapPath(filefullpath), FileMode.Create))
		        {
		            byte[] thumbnailArr = imagestream.ToArray();
		            strm.Write(thumbnailArr, 0, thumbnailArr.Length);
		            strm.Flush();
		            strm.Close();
		        }

		        return page.ResolveClientUrl(filefullpath);
		    }
		    finally 
		    {
                dbImage?.Dispose();
                thumbnailImage?.Dispose();
                imagestream?.Dispose();

            }
		}

		public static ImageCodecInfo GetImageCodec(Image i)
		{
			foreach (ImageCodecInfo codec in ImageCodecInfo.GetImageDecoders())
			{
				if (codec.FormatID == i.RawFormat.Guid)
					return codec;
			}

			return null;
		}

		public static string GetImageMimeType(Image i)
		{
			ImageCodecInfo codecInfo = GetImageCodec(i);

			if(codecInfo!=null)
			{
				return codecInfo.MimeType;
			}

			return "image/unknown";
		}

		public static string GetImageFilenameExtension(Image i)
		{
			ImageCodecInfo codecInfo = GetImageCodec(i);

			if (codecInfo != null)
			{
				string[] extentions = codecInfo.FilenameExtension.Split(';');

				if (extentions.Length > 0)
				{
					return extentions[0].Split('.')[1].ToLower();
				}
			}

			return string.Empty;
		}
	}
}
