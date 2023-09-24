using System;
using System.Drawing;
using System.Windows.Forms;
using tr.gov.tubitak.uekae.esya.api.common.util;

namespace tr.gov.tubitak.uekae.esya.api.smartcard.gui
{
    public partial class SelectorGUI : Form, ISelector
    {
        public SelectorGUI(Control aParent) : this()
        {
            this.Parent = aParent;
        }
        public SelectorGUI()
        {
            InitializeComponent();
        }
        public int getSelectedIndex()
        {
            return this.comboBox.SelectedIndex;
        }

        public int Select(string description, string[] inputs)
        {
            this.comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
            this.comboBox.Items.AddRange(inputs);
            this.comboBox.SelectedIndex = 0;

            this.labelMessage.Text = description;

            this.ShowDialog();
            return this.getSelectedIndex();
        }
    }
}
