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

    private class NotifyPrompt : Prompt<bool>
    {
        public NotifyPrompt(string message, string title)
        {
            Message = message;
            Title = title;
        }

        public override void Show()
        {
            if (BeginModal())
            {
                ImGui.Text(Message);

                if (ImGui.Button("OK"))
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
    

    private class NewMiloPrompt : Prompt<MiloFile?>
    {
        
        // directory type name, and then a tuple of the directory revision name and the revision number
        private static readonly List<(string, List<(string, uint)>)> DirectoryTypes = new()
        {
            ("ObjectDir", [("GH2", 16), ("GH2 360", 17), ("TBRB / GDRB", 22), ("RB3 / DC1", 27), ("DC2", 28)]),
            ("RndDir", [("GH2", 8), ("GH2 360", 9), ("TBRB / GDRB / RB3 / DC1 / DC2", 10)]),
            ("Character", [("GH2", 9), ("GH2 360", 10), ("TBRB / GDRB", 15), ("RB3 / DC1 / DC2", 18)]),
            ("PanelDir", [("GH2 / GH2 360", 2), ("TBRB / GDRB", 7), ("RB3 / DC1", 8)])
        };
        private static readonly Dictionary<string, uint> MiloSceneRevisions = new()
        {
            { "FreQuency", 6 },
            { "GH1", 10 },
            { "GH2 PS2", 24 },
            { "GH2 360 / RB1 / L:RB / GDRB / TBRB", 25 },
            { "RB3", 28 },
            { "DC1", 31 },
            { "DC2 / RBB / DC3", 32 },
        };

        private string[] dirTypeNames;
        private string[] revisionNames;

        private int curDirTypeIndex = 0;
        private int curDirRevisionIndex = 0;
        private int curSceneRevisionIndex = 0;

        private string newName = "";
        
        public NewMiloPrompt()
        {
            Title = "New Milo Scene";
            
            var tmpTypeNames = new List<string>();
            foreach (var dirType in DirectoryTypes)
            {
                tmpTypeNames.Add(dirType.Item1);
            }
            dirTypeNames = tmpTypeNames.ToArray();
            
            var tmpRevisionNames = new List<string>();
            foreach (var revision in MiloSceneRevisions)
            {
                tmpRevisionNames.Add($"{revision.Key} ({revision.Value})");
            }
            revisionNames = tmpRevisionNames.ToArray();
        }
        
        public override void Show()
        {
            if (BeginModal())
            {
                ImGui.Combo("Scene Version", ref curSceneRevisionIndex, revisionNames, revisionNames.Length);
                
                if (MiloSceneRevisions.Values.ToArray()[curSceneRevisionIndex] <= 10)
                {
                    curDirTypeIndex = 0;
                    ImGui.PushStyleVar(ImGuiStyleVar.Alpha, 0.5f);
                    ImGui.LabelText("Directory Type", "ObjectDir");
                    ImGui.PopStyleVar();
                }
                else
                {
                    if (ImGui.Combo("Directory Type", ref curDirTypeIndex, dirTypeNames, dirTypeNames.Length))
                    {
                        curDirRevisionIndex = 0;
                    }
                }

                string[] dirRevisionNames = new string[DirectoryTypes[curDirTypeIndex].Item2.Count];
                for (int i = 0; i < dirRevisionNames.Length; i++)
                {
                    var rev = DirectoryTypes[curDirTypeIndex].Item2[i];
                    dirRevisionNames[i] = $"{rev.Item1} ({rev.Item2})";
                }
                
                ImGui.Combo("Directory Revision", ref curDirRevisionIndex, dirRevisionNames, dirRevisionNames.Length);

                ImGui.InputText("Directory Name", ref newName, 128);
                
                ImGui.Separator();
                if (ImGui.Button("OK"))
                {
                    var meta = DirectoryMeta.New(DirectoryTypes[curDirTypeIndex].Item1, newName, MiloSceneRevisions.Values.ToArray()[curSceneRevisionIndex], (ushort)DirectoryTypes[curDirTypeIndex].Item2[curDirRevisionIndex].Item2);
                    Complete(new MiloFile(meta));
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
    /// Shows a notification prompt, simply conveying information to the user.
    /// </summary>
    /// <param name="message">The text to show the user.</param>
    /// <param name="title">The title of the prompt window.</param>
    /// <returns></returns>
    public static async void ShowNotifyPrompt(string message, string title)
    {
        await ShowGenericPrompt(new NotifyPrompt(message, title));
    }
    
    /// <summary>
    /// Shows a new file prompt, creating a MiloFile to be loaded.
    /// </summary>
    /// <returns>A Milo scene according to the parameters specified by the user, or null if canceled</returns>
    public static async Task<MiloFile?> ShowNewFilePrompt()
    {
        return await ShowGenericPrompt(new NewMiloPrompt());
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