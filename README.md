## reduce PDF image size to reduce the PDF file size

## usage
```csharp
var pdf=new CMPDF.PDF();
byte[] pdfBytes = File.ReadAllBytes("D:/Temp/Test.pdf");
byte[] scaledPdfBytes = pdf.compression(pdfBytes);
Console.WriteLine(scaledPdfBytes.Length);
File.WriteAllBytes("D:/Temp/Test_Resize.pdf",scaledPdfBytes);
``` 