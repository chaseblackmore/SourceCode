using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Amazon.Lambda.Core;
using System.Net.Http;
using Alexa.NET.Response;
using Alexa.NET.Request.Type;
using Alexa.NET.Request;


// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace AlexaSkillProjectStarWarsCharacters
{
    public class Function
    {

        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        //public string FunctionHandler(string input, ILambdaContext context)
        //{
        //    return input?.ToUpper();
        //}
        //CHASE BLACKMORE AND CANON FRYE
        private static HttpClient httpClient;

        public Function()
        {
            httpClient = new HttpClient();
        }


        public async Task<SkillResponse> FunctionHandler(SkillRequest input, ILambdaContext context)
        {
            var requestType = input.GetRequestType();
            string outputText = "";
            if (requestType == typeof(LaunchRequest))
            {
                return BodyResponse("Welcome to the Star wars Dictionary, say the name of a character to learn more!", false);
            }
            else if (requestType == typeof(IntentRequest))
            {
                var intent = input.Request as IntentRequest;
                if (intent.Intent.Name.Equals("PlayerIntent"))
                {

                    var characterRequest = intent.Intent.Slots["character"].Value;


                    if (characterRequest == null)
                    {
                        return BodyResponse("I did not understand the last name of the Character you asked about, please try again", false);
                    }
                    else if (intent.Intent.Name.Equals("AMAZON.StopIntent"))
                    {
                        return BodyResponse("You have now exited the Star Wars app", false);
                    }
                     


                    var characterInfo = await GetCharInfo(characterRequest, context);

                    {

                        outputText = $"{characterInfo.name} is a {characterInfo.species} from {characterInfo.homeworld}. He is {characterInfo.height} centimeters tall and weighs {characterInfo.mass} kilograms";

                    }


                }
            }
            return BodyResponse(outputText, true);
        }

        private SkillResponse BodyResponse(string outputSpeech, bool shouldEndSession, string repromptText = "Just say, tell me about Yoda to learn more. To exit, say exit.")
        {
            var response = new ResponseBody
            {
                ShouldEndSession = shouldEndSession,
                OutputSpeech = new PlainTextOutputSpeech { Text = outputSpeech }
            };

            if (repromptText != null)
            {
                response.Reprompt = new Reprompt() { OutputSpeech = new PlainTextOutputSpeech() { Text = repromptText } };
            }

            var skillResponse = new SkillResponse
            {
                Response = response,
                Version = "1.0"
            };
            return skillResponse;
        }

        private async Task<Characters> GetCharInfo(string charactername, ILambdaContext context)
        {
            Characters chars = new Characters();
            var uri = new Uri($"https://swapi.co/api/people/?search={charactername}");

            try
            {
                var response = await httpClient.GetStringAsync(uri);
                context.Logger.LogLine($"Response from URL:\n{response}");
                chars = JsonConvert.DeserializeObject<List<Characters>>(response).FirstOrDefault();
            }
            catch (Exception ex)
            {
                context.Logger.LogLine($"\nException: {ex.Message}");
                context.Logger.LogLine($"\nStack Trace: {ex.StackTrace}");
            }

            return chars;
        }
    }
}

