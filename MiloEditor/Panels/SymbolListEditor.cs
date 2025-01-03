using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MiloEditor.Panels
{
    public partial class SymbolListEditor : UserControl
    {
        private List<Symbol> symbols;
        public event EventHandler SymbolsChanged;
        public event EventHandler SymbolRemoved;

        // Method to invoke SymbolsChanged
        protected virtual void OnSymbolsChanged()
        {
            SymbolsChanged?.Invoke(this, EventArgs.Empty);
        }

        // Method to invoke SymbolRemoved
        protected virtual void OnSymbolRemoved()
        {
            SymbolRemoved?.Invoke(this, EventArgs.Empty);
        }

        public SymbolListEditor()
        {
            this.symbols = new();
            InitializeComponent();
        }

        private void SymbolListEditor_Load(object sender, EventArgs e)
        {

            addButton.Click += (s, ev) =>
            {
                dataGridView1.Rows.Add("New Symbol");
                symbols.Add(new Symbol((uint)"New Symbol".Length, "New Symbol"));
                OnSymbolsChanged();
            };

            removeButton.Click += (s, ev) =>
            {
                dataGridView1.Rows.RemoveAt(dataGridView1.RowCount - 1);
                symbols.RemoveAt(dataGridView1.RowCount - 1);
                OnSymbolsChanged();
                OnSymbolRemoved();
            };

            dataGridView1.CellValueChanged += (s, ev) =>
            {
                if (ev.RowIndex >= 0 && ev.RowIndex < symbols.Count)
                {
                    string newValue = dataGridView1.Rows[ev.RowIndex].Cells[ev.ColumnIndex].Value?.ToString();
                    if (!string.IsNullOrEmpty(newValue))
                    {
                        symbols[ev.RowIndex] = new Symbol((uint)newValue.Length, newValue);
                        OnSymbolsChanged();
                    }
                }
            };
        }

        private void dataGridView1_Resize(object sender, EventArgs e)
        {
            int buttonWidth = flowLayoutPanel1.ClientSize.Width / 2;
            addButton.Width = buttonWidth;
            removeButton.Width = buttonWidth;
        }

        public void SetSymbols(List<Symbol> symbols)
        {
            dataGridView1.Columns.Clear();
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Symbols", Name = "fieldColumn", AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells });
            this.symbols = symbols;
            dataGridView1.Rows.Clear();
            foreach (Symbol symbol in symbols)
            {
                dataGridView1.Rows.Add(symbol.value);
            }
        }
    }
}
