namespace CMPDF;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using iTextSharp.text.pdf.parser;
using System.Drawing;
public class PDF
{
    public byte[] compression(byte[] sourcepdf)
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
                                var pdfImage = new PdfImageObject((PRStream)pdfStrem);
                                //var img = pdfImage.GetDrawingImage();    
                                var pdfimg = pdfImage.GetImageAsBytes();
                                pdfimg = ResizeImage(pdfimg, 800);

                                iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(pdfimg);

                                PdfReader.KillIndirect(obj);
                                iTextSharp.text.Image maskImage = img.ImageMask;
                                if (maskImage != null) writer.AddDirectImageSimple(maskImage);
                                writer.AddDirectImageSimple(img, (PRIndirectReference)obj);
                                //break;
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
}
