using Blazored.LocalStorage;
using Blazored.Toast;
using Blazorise;
using Blazorise.Bootstrap;
using Blazorise.Icons.FontAwesome;
using Ed.Bannerboard.UI;
using Ed.Bannerboard.UI.Logic;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services
    .AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) })
    .AddSingleton(sp => new AppState())
    .AddBlazorise(options =>
    {
        options.Immediate = true;
		options.ProductToken = "CjxRBXB/NAE9WgN3fDQ1BlEAc3g0CT1XAnB6Mws6bjoNJ2ZdYhBVCCo/CjVUAUxERldhE1EvN0xcNm46FD1gSkUHCkxESVFvBl4yK1FBfAYKAiFoVXkNWTU3CDJTPHQAGkR/Xip0HhFIeVQ8bxMBUmtTPApwfjUIAWlvHg9QbEMgfwweSX1YJm8eA0RgUzxiDhlWZ1NZAXF+NTUGPG8CBkRqWDBvHgNEYFM8Yg4ZVmdTWQFxQw9nUy95EhpTcUk0bx4DRGBTPGIOGVZnU1kBcX41NQY8bxUcQH1aKnUWEVp1TTtvHhxKb188b3t/NQgBaXt0D1NfJ0hbdxhSfENVRi86XAE/BAcIe1F5RilRBTlJDVY2aCojTlZhFl4sFDJLeihgeXpHTWMMAzMeN2k6AWgCO1JyTQFYeTtASE9Ve3N6TH1HKlMoO0MOSA8Bdn9GDCcZSCAKV1NNJVYgOGF7Yg1GOQxxaUQgCA4/Yl5jCVJ4emt5Txl6NydOflY7Xwt/SkFADko3IEt6ZlRbFX1AUCcUfDQtdU0+L3wzejg=";
	})
    .AddBootstrapProviders()
    .AddFontAwesomeIcons()
    .AddBlazoredLocalStorage()
    .AddBlazoredToast();

await builder.Build().RunAsync();
