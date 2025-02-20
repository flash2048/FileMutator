using System.Net;
using FileMutator.IntegrationTest.Generators;
using System.Text;
using FluentAssertions;

namespace FileMutator.IntegrationTest
{
    [Collection("JobServiceFixtureAspire")]
    public class FileTest
    {
        private readonly V1Client _client;

        public FileTest(JobServiceFixtureAspire jobServiceFixtureAspire)
        {
            _client = new V1Client(new HttpClient())
            {
                BaseUrl = jobServiceFixtureAspire.BaseAddress!.AbsoluteUri
            };
        }

        [Fact]
        public async Task FileUploadAndMutationTest()
        {
            var str = "Hello, World!";
            var contentType = "text/plain";
            var fileName = "hello.txt";
            Guid fileId;

            // upload
            {
                var byteArray = Encoding.UTF8.GetBytes(str);

                using var ms = new MemoryStream(byteArray);
                var file = new FileParameter(ms, fileName, contentType);
                var fileInfoShort = await _client.UploadAsync(file);

                fileInfoShort.ContentType.Should().Be(contentType);
                fileInfoShort.Name.Should().Be(fileName);
                fileInfoShort.Size.Should().Be(str.Length);
                fileInfoShort.UploadedAt.Should().BeAfter(DateTimeOffset.UtcNow.AddMinutes(-1));
                fileInfoShort.IsMutated.Should().BeFalse();

                fileId = fileInfoShort.Id;
            }

            // download
            {
                var file = await _client.DownloadAsync(fileId);
                using MemoryStream memoryStream = new MemoryStream();
                await file.Stream.CopyToAsync(memoryStream);
                var txt = Encoding.UTF8.GetString(memoryStream.ToArray());
                txt.Should().Be(str);
            }

            // mutate
            {
                var fileInfoFull = await _client.MutateAsync(fileId);
                var newText = fileInfoFull.FileText;

                fileInfoFull.Id.Should().Be(fileId);
                fileInfoFull.Name.Should().Be(fileName);
                fileInfoFull.Size.Should().NotBe(str.Length);
                fileInfoFull.IsMutated.Should().BeTrue();
                fileInfoFull.FileText.Should().NotBe(str);

                fileInfoFull = await _client.MutateAsync(fileId);
                fileInfoFull.Size.Should().NotBe(newText.Length);
                fileInfoFull.IsMutated.Should().BeTrue();
                fileInfoFull.FileText.Should().NotBe(newText);

                str = fileInfoFull.FileText;
            }

            // file info
            {
                var fileInfoFull = await _client.FilesAsync(fileId);
                fileInfoFull.Name.Should().Be(fileName);
                fileInfoFull.Size.Should().Be(str.Length);
                fileInfoFull.FileText.Should().Be(str);
                fileInfoFull.IsMutated.Should().BeTrue();
            }
        }

        [Fact]
        public async Task NegativeCasesTest()
        {
            var contentType = "text/plain";
            var fileName = "hello.txt";

            // empty file
            {
                var byteArray = Encoding.UTF8.GetBytes(string.Empty);

                using var ms = new MemoryStream(byteArray);
                var file = new FileParameter(ms, fileName, contentType);

                var error = await Assert.ThrowsAsync<ApiException>(() => _client.UploadAsync(file));
                error.StatusCode.Should().Be((int)HttpStatusCode.BadRequest);
            }

            // mutate - not found Id
            {
                var error = await Assert.ThrowsAsync<ApiException>(() => _client.MutateAsync(Guid.NewGuid()));
                error.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
            }

            // download - not acceptable
            {
                var error = await Assert.ThrowsAsync<ApiException>(() => _client.DownloadAsync(Guid.NewGuid()));
                error.StatusCode.Should().Be((int)HttpStatusCode.NotAcceptable);
            }

            // file info - not found Id
            {
                var error = await Assert.ThrowsAsync<ApiException>(() => _client.FilesAsync(Guid.NewGuid()));
                error.StatusCode.Should().Be((int)HttpStatusCode.NotFound);
            }
        }
    }
}
