using ImGuiNET;
using MiloLib;
using MiloLib.Assets;
using MiloLib.Classes;
using MiloLib.Utils;
using Vector2 = System.Numerics.Vector2;

namespace ImMilo;

public partial class Program
{
    /// <summary>
    /// A prompt. Used for confirmation, additional info, etc. This non-generic class is likely not what you want.
    /// Check <see cref="Prompt{T}"/>
    /// </summary>
    public abstract class Prompt
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

    /// <summary>
    /// A prompt that returns a value when completed.
    /// </summary>
    /// <typeparam name="T">Target value to return.</typeparam>
    public abstract class Prompt<T> : Prompt
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
            Title = "Save As";
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

    private class TextPrompt : Prompt<string?>
    {
        private string value;
        
        public TextPrompt(string message, string title, string defaultValue = "")
        {
            Message = message;
            Title = title;
            value = defaultValue;
        }

        public override void Show()
        {
            if (BeginModal())
            {
                if (ImGui.IsWindowAppearing())
                {
                    ImGui.SetKeyboardFocusHere();
                }
                ImGui.InputText(Message, ref value, 128);

                if (ImGui.Button("Ok"))
                {
                    Complete(value);
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

    /// <summary>
    /// Shows a <see cref="Prompt{T}"/>, and waits for it to be completed by the user.
    /// </summary>
    /// <param name="prompt">The prompt to be shown to the user.</param>
    /// <typeparam name="T">Return type of the prompt.</typeparam>
    /// <returns>A task that waits for the user to complete the task.</returns>
    public static async Task<T> ShowGenericPrompt<T>(Prompt<T> prompt)
    {
        Prompt.prompts.Enqueue(prompt);
        return await prompt.CompletionSource.Task;
    }

    /// <summary>
    /// Shows a confirmation prompt, asking the user to answer a yes or no question.
    /// </summary>
    /// <param name="message">The question to ask the user.</param>
    /// <returns></returns>
    public static async Task<bool> ShowConfirmPrompt(string message)
    {
        return await ShowGenericPrompt(new ConfirmPrompt(message));
    }

    /// <summary>
    /// Shows a text prompt, asking the user for a string.
    /// </summary>
    /// <param name="inputLabel">Label shown next to the text box.</param>
    /// <param name="title">Window title.</param>
    /// <param name="defaultValue">Value first entered in the text box.</param>
    /// <returns></returns>
    public static async Task<string?> ShowTextPrompt(string inputLabel, string title, string defaultValue = "")
    {
        return await ShowGenericPrompt(new TextPrompt(inputLabel, title, defaultValue));
    }

    /// <summary>
    /// Shows a save prompt, allowing the user to modify <see cref="SaveSettings"/>.
    /// </summary>
    /// <param name="target">The file to retrieve information from.</param>
    /// <returns></returns>
    public static async Task<SaveSettings?> ShowSavePrompt(MiloFile target)
    {
        return await ShowGenericPrompt(new MiloSaveSettingsPrompt(target));
    }

    private static void ProcessPrompts()
    {
        if (Prompt.prompts.Count > 0)
        {
            var prompt = Prompt.prompts.Peek();
            if (!prompt.Opened)
            {
                ImGui.OpenPopup(prompt.Title);
                prompt.Opened = true;
            }
            prompt.Show();
        }
    }
}