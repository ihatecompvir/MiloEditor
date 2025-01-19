using ImGuiNET;
using MiloLib;
using MiloLib.Assets;
using MiloLib.Classes;
using MiloLib.Utils;
using Vector2 = System.Numerics.Vector2;

namespace ImMilo;

public partial class Program
{

    private abstract class Prompt
    {
        public string Message;
        public bool Opened;
        public static Queue<Prompt> prompts = new();
        public string Title = "Prompt";

        /// <summary>
        /// Internal method to complete the prompt. Do not call without an argument from <see cref="Prompt{T}"/> prompts,
        /// they will not complete their associated task and stall whatever was waiting on them.
        /// </summary>
        protected virtual void Complete()
        {
            ImGui.CloseCurrentPopup();
            prompts.Dequeue();
            Opened = false;
        }

        public abstract void Show();

        protected bool BeginModal(ImGuiWindowFlags toggleFlags = ImGuiWindowFlags.None) => ImGui.BeginPopupModal(Title, ImGuiWindowFlags.AlwaysAutoResize ^ toggleFlags);
    }
    
    private abstract class Prompt<T> : Prompt
    {
        public TaskCompletionSource<T> CompletionSource = new();

        protected override void Complete()
        {
            base.Complete();
            CompletionSource.SetCanceled();
        }

        protected void Complete(T result)
        {
            base.Complete();
            CompletionSource.SetResult(result);
        }
    }
    
    private class ConfirmPrompt : Prompt<bool>
    {
        public ConfirmPrompt(string message)
        {
            Message = message;
            Title = "Confirm";
        }

        public override void Show()
        {
            if (BeginModal())
            {
                ImGui.Text(Message);
                
                if (ImGui.Button("Yes"))
                {
                    Complete(true);
                }
                ImGui.SameLine();
                if (ImGui.Button("No"))
                {
                    Complete(true);
                }
                ImGui.EndPopup();
            }
        }
    }

    public class SaveSettings
    {
        [Name("Compression Type")]
        public MiloFile.Type compressionType;

        [Name("Endianness")]
        public Endian endianness;

        [Name("Platform"), Description("Platform conversions will likely not work.")]
        public DirectoryMeta.Platform platform;
    }
    
    private class MiloSaveSettingsPrompt : Prompt<SaveSettings?>
    {

        private MiloFile target;
        private SaveSettings settings;
        
        public MiloSaveSettingsPrompt(MiloFile target)
        {
            Message = "Save";
            Title = "Save";
            this.target = target;
            settings = new SaveSettings
            {
                compressionType = target.compressionType,
                platform = target.dirMeta.platform,
                endianness = target.endian
            };
        }

        public override void Show()
        {
            if (BeginModal())
            {
                ImGui.BeginChild("settings", new Vector2(700, ImGui.GetFrameHeight()*5.5f));
                EditorPanel.Draw(settings, 0, false);
                ImGui.EndChild();

                if (ImGui.Button("Save"))
                {
                    Complete(settings);
                }
                ImGui.SameLine();
                if (ImGui.Button("Cancel"))
                {
                    Complete(null);
                }
                ImGui.EndPopup();
            }
        }
    }

    public static async Task<bool> ShowConfirmPrompt(string message)
    {
        var prompt = new ConfirmPrompt(message);
        Prompt.prompts.Enqueue(prompt);
        return await prompt.CompletionSource.Task;
    }

    public static async Task<SaveSettings?> ShowSavePrompt(MiloFile target)
    {
        var prompt = new MiloSaveSettingsPrompt(target);
        Prompt.prompts.Enqueue(prompt);
        return await prompt.CompletionSource.Task;
    }

    public static void ProcessPrompts()
    {
        if (Prompt.prompts.Count > 0)
        {
            var prompt = Prompt.prompts.Peek();
            if (!prompt.Opened)
            {
                Console.WriteLine($"Opening prompt {prompt.Title}");
                ImGui.OpenPopup(prompt.Title);
                prompt.Opened = true;
            }
            prompt.Show();
        }
    }
}