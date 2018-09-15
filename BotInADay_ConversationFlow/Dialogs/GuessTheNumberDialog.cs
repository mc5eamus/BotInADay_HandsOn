using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;

namespace BotInADay_ConversationFlow.Dialogs
{
    [Serializable]
    public class GuessTheNumberDialog : IDialog<int>
    {
        private readonly int number = 0;
        private int counter = 0;
        private const string ValidationError = "I'm certain it's a number between 1 and 100. Give it another try!";

        public GuessTheNumberDialog(int number)
        {
            this.number = number;
        }

        public async Task StartAsync(IDialogContext context)
        {
            PromptDialog.Number(context, ResponseReceivedAsync,
                "I have a number between 1 and 100 in mind. Can you guess it?", 
                ValidationError, 10, null, 1, 100);
        }

        private async Task ResponseReceivedAsync(IDialogContext context, IAwaitable<long> result)
        {
            var guess = await result;
            counter++;
            if (guess == number)
            {
                await context.PostAsync($"Exactly! Well done!");
                context.Done(counter);
                return;
            }
            var hint = number > guess ? "bigger" : "smaller";

            PromptDialog.Number(context, ResponseReceivedAsync,
                $"Not quite. Try a {hint} one",
                ValidationError, 3, null, 1, 100);
        }
    }
}