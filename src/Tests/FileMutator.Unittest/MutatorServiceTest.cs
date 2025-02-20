using System.Text;
using FileMutator.Tools;
using FileMutator.Tools.Interfaces;
using FluentAssertions;

namespace FileMutator.UnitTest
{
    [Collection("JobServiceFixtureAspire")]
    public class MutatorServiceTest
    {
        private readonly IMutatorService _mutatorService;
        public MutatorServiceTest()
        {
            _mutatorService = new MutatorService();
        }

        [Fact]
        public void MutatorServiceTextTest()
        {
            var str = "Hello, World!";

            {
                var newTxt = _mutatorService.MutateText(str);
                newTxt.Should().NotBe(str);
                newTxt.Should().Contain(str);
            }

            {
                var data = Encoding.UTF8.GetBytes(str);
                var newData = _mutatorService.MutateText(data);
                var newTxt = Encoding.UTF8.GetString(newData);
                newTxt.Should().NotBe(str);
                newTxt.Should().Contain(str);
            }
        }
    }
}
