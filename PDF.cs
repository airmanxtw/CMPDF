﻿namespace CMPDF;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using iTextSharp.text.pdf.parser;
using System.Drawing;
public class PDF
{
    public byte[] compression(byte[] sourcepdf, int maxWidth = 800)
    {
        MemoryStream ms = new MemoryStream();
        PdfReader pdf = new PdfReader(sourcepdf);

        PdfStamper stp = new PdfStamper(pdf, ms);
        PdfWriter writer = stp.Writer;

        for (int pageNumber = 1; pageNumber <= pdf.NumberOfPages; pageNumber++)
        {
            PdfDictionary pg = pdf.GetPageN(pageNumber);
            PdfDictionary res = (PdfDictionary)PdfReader.GetPdfObject(pg.Get(PdfName.RESOURCES));
            PdfDictionary xobj = (PdfDictionary)PdfReader.GetPdfObject(res.Get(PdfName.XOBJECT));
            if (xobj != null)
            {
                foreach (PdfName name in xobj.Keys)
                {
                    PdfObject obj = xobj.Get(name);
                    if (obj.IsIndirect())
                    {
                        PdfDictionary tg = (PdfDictionary)PdfReader.GetPdfObject(obj);
                        if (tg != null)
                        {
                            PdfName type = (PdfName)PdfReader.GetPdfObject(tg.Get(PdfName.SUBTYPE));

                            if (PdfName.IMAGE.Equals(type))
                            {

                                int XrefIndex = Convert.ToInt32(((PRIndirectReference)obj).Number.ToString(System.Globalization.CultureInfo.InvariantCulture));
                                PdfObject pdfObj = pdf.GetPdfObject(XrefIndex);

                                PdfStream pdfStrem = (PdfStream)pdfObj;
                                byte[] pdfimg = PdfReader.GetStreamBytesRaw((PRStream)pdfStrem);

                                if (IsImage(pdfimg))
                                {                                   
                                    pdfimg = ResizeImage(pdfimg, maxWidth);
                                    iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(pdfimg);
                                    PdfReader.KillIndirect(obj);
                                    iTextSharp.text.Image maskImage = img.ImageMask;
                                    if (maskImage != null) writer.AddDirectImageSimple(maskImage);
                                    writer.AddDirectImageSimple(img, (PRIndirectReference)obj);                                    
                                }
                            }
                            else if (PdfName.FORM.Equals(type))
                            {

                            }
                        }

                    }
                }
            }
        }
        stp.Close();
        return ms.ToArray();
    }

    private byte[] ResizeImage(byte[] sourceimg, int maxwidth)
    {
        MemoryStream sourcemem = new MemoryStream(sourceimg);
        var img = System.Drawing.Image.FromStream(sourcemem);
        System.Drawing.Image image = img;
        var _width = image.Width;
        var _height = image.Height;

        if ((_width > maxwidth))
        {
            var newWidth = maxwidth;
            var newHeight = System.Convert.ToInt32(_height / (double)_width * maxwidth);

            System.Drawing.Bitmap thumbnailBitmap = new System.Drawing.Bitmap(newWidth, newHeight);
            System.Drawing.Graphics thumbnailGraph = System.Drawing.Graphics.FromImage(thumbnailBitmap);

            thumbnailGraph.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
            thumbnailGraph.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            thumbnailGraph.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.High;

            System.Drawing.Rectangle imageRectangle = new System.Drawing.Rectangle(0, 0, newWidth, newHeight);
            thumbnailGraph.DrawImage(image, imageRectangle);


            image.Dispose();
            MemoryStream mem = new MemoryStream();
            thumbnailBitmap.Save(mem, System.Drawing.Imaging.ImageFormat.Jpeg);
            return mem.ToArray();
        }
        else
            return sourceimg;
    }

    private string GetImageFormat(byte[] byteArray)
    {
        try
        {
            const int INT_SIZE = 4;

            var bmp = System.Text.Encoding.ASCII.GetBytes("BM");     // BMP
            var gif = System.Text.Encoding.ASCII.GetBytes("GIF");    // GIF
            var png = new byte[] { 137, 80, 78, 71 };                // PNG
            var tiff = new byte[] { 73, 73, 42 };                    // TIFF
            var tiff2 = new byte[] { 77, 77, 42 };                   // TIFF
            var jpeg = new byte[] { 255, 216, 255, 224 };            // jpeg
            var jpeg2 = new byte[] { 255, 216, 255, 225 };           // jpeg2 (canon)
             
            var buffer = new byte[INT_SIZE];
            System.Buffer.BlockCopy(byteArray, 0, buffer, 0, INT_SIZE);

            if (bmp.SequenceEqual(buffer.Take(bmp.Length)))
                return "bmp";

            if (gif.SequenceEqual(buffer.Take(gif.Length)))
                return "gif";

            if (png.SequenceEqual(buffer.Take(png.Length)))
                return "png";

            if (tiff.SequenceEqual(buffer.Take(tiff.Length)))
                return "tiff";

            if (tiff2.SequenceEqual(buffer.Take(tiff2.Length)))
                return "tiff";

            if (jpeg.SequenceEqual(buffer.Take(jpeg.Length)))
                return "jpeg";

            if (jpeg2.SequenceEqual(buffer.Take(jpeg2.Length)))
                return "jpeg";

            return "unknown";
        }
        catch (Exception)
        {
            return "unknown";
        }
    }

    private bool IsImage(byte[] byteArray)
    {
        return GetImageFormat(byteArray) != "unknown" ? true : false;
    }
}
