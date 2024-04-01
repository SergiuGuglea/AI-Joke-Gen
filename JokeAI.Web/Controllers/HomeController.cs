using Microsoft.AspNetCore.Mvc;
using OpenAI_API;
using OpenAI_API.Completions;

namespace JokeAI.Web.Controllers
{
    public class HomeController : Controller
    {
        private string Result = "";
        private string JokeEvaluation = "";
        public IActionResult Index()
        {
            ViewData["TokenResult"] = Result;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Index(string topic, string tone, string kind)
        {
            string prompt = $"Generate a {tone} {kind} joke about {topic}.";

            OpenAIAPI api = new("") // this is the implementatin of OpenAI, it requires a key, put empty estring for local host
            {
                ApiUrlFormat = "http://127.0.0.1:5000/{0}/{1}" //normally, define it in appsettings
            };

            await foreach (var token in api.Completions.StreamCompletionEnumerableAsync(new CompletionRequest(prompt, null, 512, 0.3, presencePenalty: 0.1, frequencyPenalty: 0.1)))
            {
                Result += token; //it should use signalR for streaming, for now just wait for completition
            }


            //evaluate this joke
            string evaluatePrompt = $"What do think about this joke? {Result.Replace(prompt, "")}";


            await foreach (var token in api.Completions.StreamCompletionEnumerableAsync(new CompletionRequest(evaluatePrompt, null, 1024, 0.3, presencePenalty: 0.1, frequencyPenalty: 0.1)))
            {
                JokeEvaluation += token;
            }



            ViewData["TokenResult"] = Result.Replace(prompt, "");
            ViewData["Evaluation"] = JokeEvaluation.Replace(evaluatePrompt, "");
            return View();
        }
    }
}
