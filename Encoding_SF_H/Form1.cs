using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Encoding_SF_H
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private Tree GetChild(Stack<KeyValuePair<string, float>> data)
        {
            if (data.Count == 1)
            {
                return new Tree(data.Peek().Key);
            }
            var sum = data.Sum((x) => { return x.Value; });
            var half = sum / 2;

            var leftS = new Stack<KeyValuePair<string, float>>();
            leftS.Push(data.Pop());

            var firstSum = leftS.Peek().Value;
            while (data.Count > 1)
            {
                var top = data.Peek();
                if (firstSum + top.Value > half + top.Value / 2)
                {
                    break;
                }
                firstSum += top.Value;
                leftS.Push(top);
                data.Pop();
            }

            var result = new Tree();
            result.Left = GetChild(leftS);
            result.Right = GetChild(data);

            return result;
        }

        private Dictionary<string, string> GetEncoded(Tree root)
        {
            var res = new Dictionary<string, string>();
            if (root.Signal != null)
            {
                res.Add(root.Signal, "");
                return res;
            }

            var left = GetEncoded(root.Left);
            var right = GetEncoded(root.Right);

            foreach (var node in left)
            {
                res.Add(node.Key, node.Value + "0");
            }
            foreach (var node in right)
            {
                res.Add(node.Key, node.Value + "1");
            }
            return res;
        }

        /// <summary>
        /// Shennon-Fano encoding.
        /// </summary>
        /// <param name="data">"Array" of data.</param>
        /// <returns>Encoded sequences.</returns>
        private Dictionary<string, string> encode_SF(Stack<KeyValuePair<string, float>> data)
        {
            // Shennon-Fano encoding
            // Split in half over and over again

            var treeRoot = GetChild(data);

            return GetEncoded(treeRoot);
        }

        private List<KeyValuePair<string, float>> Data;

        private void button1_Click(object sender, EventArgs e)
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "Text Files (*.txt)|*.txt";
            switch (dialog.ShowDialog())
            {
                case DialogResult.OK: break;
                default: return;
            }

            Data = new List<KeyValuePair<string, float>>();
            var input = new System.IO.StreamReader(dialog.FileName);
            while (!input.EndOfStream)
            {
                var line = input.ReadLine();
                var match = System.Text.RegularExpressions.Regex.Match(line, @"^(.+)[\s\t](.+)$");
                if (!match.Success)
                {
                    MessageBox.Show("Corrupted data!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    input.Close();
                    return;
                }
                try
                {
                    var signal = match.Groups[1].Value;
                    var probability = float.Parse(match.Groups[2].Value);

                    Data.Add(new KeyValuePair<string, float>(signal, probability));
                }
                catch (System.Exception)
                {
                    MessageBox.Show("Ошибка чтения данных. Системный разделитель (" + System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator + ") не соответствует использованному.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    input.Close();
                    return;
                }
            }
            input.Close();

            listBox1.Items.Clear();
            listBox2.Items.Clear();
            listBox3.Items.Clear();

            foreach (var element in Data)
            {
                listBox1.Items.Add(element.Key + "\t|\t" + element.Value);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (Data == null) return;
            listBox2.Items.Clear();
            var sortedStackOfData = new Stack<KeyValuePair<string, float>>(Data.OrderBy((x) =>
            {
                return x.Value;
            }));

            var encoded = encode_SF(sortedStackOfData);
            var sorted = encoded.OrderBy((x) => { return x.Key; });
            foreach (var element in sorted)
            {
                listBox2.Items.Add(element.Key + "\t|\t" + element.Value);
            }
            if (checkBox1.Checked)
            {
                var dialog = new SaveFileDialog();
                dialog.Filter = "Text file (*.txt)|*.txt";
                switch (dialog.ShowDialog())
                {
                    default: return;
                    case DialogResult.OK:
                        break;
                }

                var output = new System.IO.StreamWriter(dialog.FileName);
                foreach (var element in sorted)
                {
                    output.WriteLine(element.Key + "\t" + element.Value);
                }
                output.Close();
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (Data == null) return;
            listBox3.Items.Clear();

            var leaves = new List<Tree>();
            foreach (var el in Data)
            {
                var tr = new Tree(el.Key);
                tr.Value = el.Value;
                leaves.Add(tr);
            }
            Comparison<Tree> comparer = (a, b) => { return a.Value > b.Value ? 1 : -1; };
            leaves.Sort(comparer);

            while (leaves.Count > 1)
            {
                var newLeaf = new Tree();
                newLeaf.Value = leaves[0].Value + leaves[1].Value;
                newLeaf.Left = leaves[0];
                newLeaf.Right = leaves[1];

                leaves.RemoveAt(0);
                leaves.RemoveAt(0);

                leaves.Add(newLeaf);
                leaves.Sort(comparer);
            }

            var res = GetEncoded(leaves[0]);
            var sorted = res.OrderBy((x) => { return x.Key; });
            foreach (var element in sorted)
            {
                listBox3.Items.Add(element.Key + "\t|\t" + element.Value);
            }
            if (checkBox2.Checked)
            {
                var dialog = new SaveFileDialog();
                dialog.Filter = "Text file (*.txt)|*.txt";
                switch (dialog.ShowDialog())
                {
                    default: return;
                    case DialogResult.OK:
                        break;
                }

                var output = new System.IO.StreamWriter(dialog.FileName);
                foreach (var element in sorted)
                {
                    output.WriteLine(element.Key + "\t" + element.Value);
                }
                output.Close();
            }
        }
    }
}
