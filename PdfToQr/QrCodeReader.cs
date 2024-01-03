using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using ZXing;
using ZXing.Common;
using ZXing.ImageSharp;
using ZXing.QrCode;
using Docnet.Core;
using Docnet.Core.Models;
using Docnet.Core.Readers;

namespace PdfToQr;

public class QrCodeReader
{
    public PageDimensions PageDimension { get; set; } = new (1080, 1920);
    public Color BackgroundColor { get; set; } = Color.White;

    /// <summary>
    /// Reads a qr code from a specific page in a Pdf File
    /// Possibility to add a rectangle to specify where to look for the QR Code
    /// </summary>
    /// <param name="path">Path to the Pdf</param>
    /// <param name="page">Page number (first page is 0)</param>
    /// <param name="rectangle"></param>
    /// <returns></returns>
    public string ReadQrCodeInPdfFromFileAtPage(string path, int page, Rectangle? rectangle = null)
    {
        return ReadQrCodeInPdfFromByteArrayAtPage(File.ReadAllBytes(path), page, rectangle);
    }

    /// <summary>
    /// Reads a qr code from a specific page in a Pdf File
    /// Possibility to add a rectangle to specify where to look for the QR Code
    /// </summary>
    /// <param name="byteArray"></param>
    /// <param name="page">Page number (first page is 0)</param>
    /// <param name="rectangle"></param>
    /// <returns></returns>
    public string ReadQrCodeInPdfFromByteArrayAtPage(byte[] byteArray, int page, Rectangle? rectangle = null)
    {
        using var docReader = DocLib.Instance.GetDocReader(byteArray, PageDimension);
        int count = docReader.GetPageCount();

        if (page >= count)
        {
            throw new IndexOutOfRangeException($"Page: {page} is larger than the total number of pages: {count}.");
        }
        
        using var pageReader = docReader.GetPageReader(page);
        return ReadQrFromPage(pageReader, rectangle);
    }
    
    /// <summary>
    /// Reads a qr code from a Pdf File
    /// Possibility to add a rectangle to specify where to look for the QR Code
    /// </summary>
    /// <param name="path">Path to the Pdf</param>
    /// <param name="rectangle"></param>
    /// <returns></returns>
    public string ReadQrCodeInPdfFromFile(string path, Rectangle? rectangle = null)
    {
        return ReadQrCodeInPdfFromBytes(File.ReadAllBytes(path), rectangle);
    }

    /// <summary>
    /// Reads a qr code from a Pdf File
    /// Possibility to add a rectangle to specify where to look for the QR Code
    /// </summary>
    /// <param name="byteArray">Pdf file as byte array</param>
    /// <param name="rectangle"></param>
    /// <returns></returns>
    public string ReadQrCodeInPdfFromBytes(byte[] byteArray, Rectangle? rectangle = null)
    {
        using var docReader = DocLib.Instance.GetDocReader(byteArray, PageDimension);
        int count = docReader.GetPageCount();
        
        for (int i = 0; i < count; ++i)
        {
            using var pageReader = docReader.GetPageReader(i);
            var result = ReadQrFromPage(pageReader, rectangle);

            if (string.IsNullOrWhiteSpace(result) is false)
            {
                return result;
            }
        }

        return string.Empty;
    }
    
    private string ReadQrFromPage(IPageReader pageReader, Rectangle? rectangle = null)
    {
        var rawBytes = pageReader.GetImage();

        var width = pageReader.GetPageWidth();
        var height = pageReader.GetPageHeight();

        // Converts an array of pixels into an image.
        using var img = Image.LoadPixelData<Bgra32>(rawBytes, width, height);
        
        // helps to ignore transparent background issue
        img.Mutate(x => x.BackgroundColor(BackgroundColor));

        if (rectangle.HasValue)
        {
            img.Mutate(i => i.Crop(rectangle.Value));
        }
        
        return ReadQrCode(img);
    }
    
    /// <summary>
    /// Reads a image file from the path and scans it for a QR code.
    /// The return is the data from the QR code 
    /// </summary>
    /// <param name="path">Path to the pdf file</param>
    /// <returns>The data from the QR Code</returns>
    public static string ReadQrCodeFromFile(string path)
    {
        return ReadQrCode(File.ReadAllBytes(path));
    }
    
    /// <summary>
    /// Reads an image file from the byte array and scans it for a QR code.
    /// The return is the data from the QR code 
    /// </summary>
    /// <param name="byteArray">The pdf file as a byte array</param>
    /// <returns></returns>
    public static string ReadQrCode(byte[] byteArray) 
    {
        var image = Image.Load<Bgra32>(byteArray);

        return ReadQrCode(image);
    }
    
    /// <summary>
    /// From an image, retrieve the data in the QR code from within
    /// </summary>
    /// <param name="image">Image to find the QR code in</param>
    /// <returns>The data in the QR Code</returns>
    public static string ReadQrCode(Image<Bgra32> image) 
    {
        // Create an instance of ImageSharpLuminanceSource with Rgba32 pixel format
        var luminanceSource = new ImageSharpLuminanceSource<Bgra32>(image);

        var bitmap = new BinaryBitmap(new HybridBinarizer(luminanceSource));
        var reader = new QRCodeReader();
        var result = reader.decode(bitmap, new Dictionary<DecodeHintType, object>());
        return result?.Text ?? "";
    }
}


