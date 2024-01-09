using SixLabors.ImageSharp;
using Xunit;
using Xunit.Abstractions;

namespace PdfToQr.Tests;


public class QrCodeReaderTests(ITestOutputHelper testOutputHelper)
{
    private readonly QrCodeReader _qrCodeReader = new();

    [Fact]
    public void TestReadBarcodeZxing()
    {
        var sourcePath = @"TestData\test.jpg";

        var data = QrCodeReader.ReadQrCodeFromFile(sourcePath);
        
        testOutputHelper.WriteLine(data);
        Assert.NotNull(data);
        Assert.Equal("http://commons.wikimedia.org/wiki/Commons:Mobile_app/Download/Mobile", data);
    }

    [Fact]
    public void PdfToCroppedImage()
    {
        var sourcePath = @"TestData\partofpicture.pdf";

        var rectangle = new Rectangle(450, 125, 500, 500);
        
        var data = _qrCodeReader.ReadQrCodeInPdfFromFile(sourcePath, rectangle);
        
        testOutputHelper.WriteLine(data);
        Assert.NotNull(data);
        Assert.StartsWith("<?xml version", data);
    }

    [Fact]
    public void PdfToImage()
    {
        var sourcePath = @"TestData\wikipedia.pdf";

        var data = _qrCodeReader.ReadQrCodeInPdfFromFile(sourcePath);
        
        testOutputHelper.WriteLine(data);
        Assert.NotEmpty(data);
        Assert.Equal("http://commons.wikimedia.org/wiki/Commons:Mobile_app/Download/Mobile", data);
    }
}
