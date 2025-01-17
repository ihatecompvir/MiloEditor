using ImGuiNET;

namespace ImMilo;

public partial class Program
{
    class ConfirmPrompt
    {
        public string Message;
        public TaskCompletionSource<bool> CompletionSource;
        public bool Opened;

        public ConfirmPrompt(string message)
        {
            this.Message = message;
            CompletionSource = new TaskCompletionSource<bool>();
        }
    }
    
    private static Queue<ConfirmPrompt> prompts = new();

    public static async Task<bool> ShowConfirmPrompt(string message)
    {
        var prompt = new ConfirmPrompt(message);
        prompts.Enqueue(prompt);
        return await prompt.CompletionSource.Task;
    }

    public static void ProcessPrompts()
    {
        if (prompts.Count > 0)
        {
            var prompt = prompts.Peek();
            if (!prompt.Opened)
            {
                ImGui.OpenPopup("Confirm");
                prompt.Opened = true;
            }
            if (ImGui.BeginPopupModal("Confirm", ImGuiWindowFlags.AlwaysAutoResize))
            {
                ImGui.Text(prompt.Message);
                
                if (ImGui.Button("Yes"))
                {
                    ImGui.CloseCurrentPopup();
                    prompts.Dequeue();
                    prompt.CompletionSource.SetResult(true);
                }
                ImGui.SameLine();
                if (ImGui.Button("No"))
                {
                    ImGui.CloseCurrentPopup();
                    prompts.Dequeue();
                    prompt.CompletionSource.SetResult(false);
                }
                ImGui.EndPopup();
            }
        }
    }
}