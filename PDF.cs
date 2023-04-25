﻿namespace CMPDF;
//using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using iTextSharp.text.pdf.parser;
using ImageMagick;
public class PDF
{
    public byte[] compression(byte[] sourcePdf, int imageMaxWidth = 800)
    {
        try
        {
            MemoryStream ms = new MemoryStream();
            PdfReader pdf = new PdfReader(sourcePdf);

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
                                    var pdfImage = new PdfImageObject((PRStream)pdfStrem);

                                    var isMask = pdfImage.GetDictionary().Contains(PdfName.SMASK);

                                    byte[] pdfimg = PdfReader.GetStreamBytesRaw((PRStream)pdfStrem);

                                    if (IsImage(pdfimg) && !isMask)
                                    {
                                        pdfimg = ResizeImage(pdfimg, imageMaxWidth);
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
            if (ms.ToArray().Length < sourcePdf.Length)
                return ms.ToArray();
            else
                return sourcePdf;
        }
        catch (Exception)
        {
            return sourcePdf;
        }
    }

    public byte[] ResizeImage(byte[] sourceimg, int maxwidth, bool toJpeg = true)
    {
        MemoryStream sourcemem = new MemoryStream(sourceimg);
        try
        {
            var img = new MagickImage(sourcemem);
            if ((img.Width > maxwidth))
            {
                var newWidth = maxwidth;
                var newHeight = System.Convert.ToInt32(img.Height / (double)img.Width * maxwidth);
                img.Resize(newWidth, newHeight);
                if (toJpeg) img.Format = MagickFormat.Jpeg;
                return img.ToByteArray();
            }
            else
                return sourceimg;
        }
        catch (Exception)
        {
            return sourceimg;
        }

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
