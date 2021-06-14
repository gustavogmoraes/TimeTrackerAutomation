using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using PuppeteerSharp;
using PuppeteerSharp.Contrib.Extensions;
using PuppeteerSharp.Input;

namespace TimeTrackerAutomation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TimeTracking : Controller
    {
        [HttpPost("[action]")]
        public void EnterTime([FromBody] Tracking tracking)
        {
            Task.Run(async () =>
            {
                await new BrowserFetcher().DownloadAsync(BrowserFetcher.DefaultChromiumRevision);
                var browser = await Puppeteer.LaunchAsync(new LaunchOptions
                {
                    Headless = false,
                    DefaultViewport = null,
                    SlowMo = 1
                });

                var page = (await browser.PagesAsync()).FirstOrDefault();
                if (page == null)
                {
                    return;
                }
                
                await page.GoToAsync(@"https://timetracker.bairesdev.com/");
                await page.TypeAsync("#ctl00_ContentPlaceHolder_UserNameTextBox", tracking.User);
                await page.TypeAsync("#ctl00_ContentPlaceHolder_PasswordTextBox", tracking.Password);

                await page.ClickAsync("#ctl00_ContentPlaceHolder_LoginButton");

                await page.GoToAsync(@"https://timetracker.bairesdev.com/CargaTimeTracker.aspx");
                
                await page.EvaluateExpressionAsync($@"document.querySelector('#ctl00_ContentPlaceHolder_txtFrom').value = null");
                await page.TypeAsync("#ctl00_ContentPlaceHolder_txtFrom", tracking.Date, new TypeOptions{ Delay = 30 });

                var dropProject = await page.QuerySelectorAsync("#ctl00_ContentPlaceHolder_idProyectoDropDownList");
                await dropProject.ClickAsync();
                
                var children = await dropProject.QuerySelectorAllAsync("option");
                await children[2].ClickAsync();
                    
                await page.ClickAsync("#ctl00_ContentPlaceHolder_TiempoTextBox");
                await page.TypeAsync("#ctl00_ContentPlaceHolder_TiempoTextBox", tracking.Hours.ToString(), new TypeOptions{ Delay = 30 });

                await page.ClickAsync("#ctl00_ContentPlaceHolder_idTipoAsignacionDropDownList");

                //await page.FocusAsync("#ctl00_ContentPlaceHolder_idTipoAsignacionDropDownList");
                //await page.SelectAsync("#ctl00_ContentPlaceHolder_idTipoAsignacionDropDownList", tracking.AssignmentType);

                await page.ClickAsync("#ctl00_ContentPlaceHolder_DescripcionTextBox");
                await page.TypeAsync("#ctl00_ContentPlaceHolder_DescripcionTextBox", tracking.Description, new TypeOptions{ Delay = 30 });

                await page.FocusAsync("#ctl00_ContentPlaceHolder_idFocalPointClientDropDownList");
                await page.SelectAsync("#ctl00_ContentPlaceHolder_idFocalPointClientDropDownList", tracking.FocalPoint);

                await browser.CloseAsync();

            }).ConfigureAwait(false);
        }
    }
    
    public class Tracking
    {
        public string User { get; set; }
        
        public string Password { get; set; }
        
        public string Date { get; set; }
        
        public string Project { get; set; }
        
        public int Hours { get; set; }
        
        public string AssignmentType { get; set; }
        
        public string Description { get; set; }
        
        public string FocalPoint { get; set; }
    }
}