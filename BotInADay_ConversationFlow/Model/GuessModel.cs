using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.FormFlow;
using Microsoft.Bot.Builder.FormFlow.Advanced;

namespace BotInADay_ConversationFlow.Model
{
    [Serializable]
    public class GuessModel
    {
        private int number = (new Random(Guid.NewGuid().GetHashCode())).Next(1, 101);

        [Numeric(1, 100)]
        [Prompt("Make your guess (1-100)")]
        public int Guess { get; set; }
        public int Counter { get; set; }

        public static IForm<GuessModel> BuildForm()
        {
            return new FormBuilder<GuessModel>()
                .Message("I have a number in mind. Guess away!")
                .Field(nameof(GuessModel.Guess), 
                    validate: (state, value) =>
                    {
                        var guess = (long) value;
                        state.Counter++;
                        ValidateResult result = new ValidateResult();
                        if (guess == state.number)
                        {
                            result.IsValid = true;
                            return Task.FromResult(result);
                        }

                        var hint = state.number > guess ? "bigger" : "smaller";
                        result.Feedback = $"Not quite. Try a {hint} one";
                        return Task.FromResult(result);
                    })
                .OnCompletion(async (context, order) =>
                {
                    await context.PostAsync("You did it!");
                })
                .Build();
        }
    }
}