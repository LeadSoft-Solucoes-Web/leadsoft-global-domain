using LeadSoft.Common.GlobalDomain.Entities;

namespace LeadSoft.Common.GlobalDomain.Tests
{
    public class Tests(ITestOutputHelper output)
    {
        [Fact]
        public void OverShadowEmail()
        {
            output.WriteLine("ola@agencia.red".OvershadowEmail());
            output.WriteLine("lucasr.tavares@outlook.com.br".OvershadowEmail());
            output.WriteLine("lucas@leadsoft.inf.br".OvershadowEmail());
            output.WriteLine("lucas@ecx.com.br".OvershadowEmail());
            output.WriteLine("lucasresendetavares@gmail.com.br".OvershadowEmail());
        }
    }
}
