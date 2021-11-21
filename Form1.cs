using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Forms;
using ZedGraph;
using System.Collections.Generic;

namespace goldenratio
{
    public partial class Form1 : Form
    {
        GraphPane pane;
        UserExpression expression;
        LineItem minPoint;
        List<PointPairList> minPointCoords;
        int index = -1;
        bool graphExists = false;

        public Form1()
        {
            InitializeComponent();
            pane = zedGraphControl.GraphPane;
            pane.Title.Text = "Графики";
        }
        private async void countGraphToolStripMenuItem_Click(object sender, EventArgs e)
        {
            graphExists = true;
            try
            {
                expression = new UserExpression()
                {
                    a = Convert.ToDouble(textBoxA.Text),
                    b = Convert.ToDouble(textBoxB.Text),
                    precision = Convert.ToDouble(textBoxE.Text),
                    userExpression = textBoxExpression.Text
                };

                if (double.IsNaN(expression.getPointY(expression.a)))
                {
                    throw new Exception("Неверно записана F(x)!");
                }

                if (expression.precision <= 0)
                {
                    throw new Exception("Неверно записана точность!");
                }

                if (expression.a >= expression.b)
                {
                    throw new Exception("A не может быть больше или равна B!");
                }

                Task<PointPairList> getPoints = expression.getGraphPoints();
                LineItem curve = pane.AddCurve(expression.userExpression, await getPoints, Color.Purple, SymbolType.None);
                zedGraphControl.AxisChange();
                zedGraphControl.Refresh();

                countMinToolStripMenuItem.Enabled = true;
            }
            catch (FormatException)
            {
                MessageBox.Show("Заполните пустые поля!");
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message);
            }
        }
        private async void countMinStepsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            countMinToolStripMenuItem.Enabled = false;
            Task<List<PointPairList>> getMinPointCoords = expression.getMinPointCoords();
            if (graphExists)
            {
                minPointCoords = null;
                graphExists = false;
            }
            if (minPointCoords == null)
            {
                minPointCoords = await getMinPointCoords;
            }
            buttonStepForward.Enabled = true;
        }
        private async void countMinAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            countMinToolStripMenuItem.Enabled = false;
            Task<List<PointPairList>> getMinPointCoords = expression.getMinPointCoords();
            if (graphExists)
            {
                minPointCoords = null;
                graphExists = false;
            }
            if (minPointCoords == null)
            {
                minPointCoords = await getMinPointCoords;
            }
            for (int pointIndex = 0; pointIndex < minPointCoords.Count; ++pointIndex)
            {
                pane.CurveList.Remove(minPoint);
                await Task.Delay(500);
                minPoint = buttonStepAction(pointIndex, buttonStepForward, buttonStepBack, pointIndex >= minPointCoords.Count - 1);
            }
            
            index = minPointCoords.Count - 1;

            buttonStepBack.Enabled = true;
        }
        private void buttonStepBack_Click(object sender, EventArgs e)
        {
            pane.CurveList.Remove(minPoint);
            minPoint = buttonStepAction(--index, buttonStepBack, buttonStepForward, index <= 0);
        }
        private void buttonStepForward_Click(object sender, EventArgs e)
        {
            pane.CurveList.Remove(minPoint);
            minPoint = buttonStepAction(++index, buttonStepForward, buttonStepBack, index >= minPointCoords.Count - 1);
        }
        private LineItem buttonStepAction(int index, Button button, Button otherButton, bool condition)
        {
            if (condition == true)
            {
                button.Enabled = false;
                otherButton.Enabled = true;
            }
            else
            {
                button.Enabled = true;
                otherButton.Enabled = true;
                minPoint = pane.AddCurve("", minPointCoords[index], Color.BlueViolet, SymbolType.Circle);
                zedGraphControl.AxisChange();
                zedGraphControl.Refresh();
                textBoxMinPointX.Text = minPointCoords[index][0].X.ToString();
                textBoxMinPointY.Text = minPointCoords[index][0].Y.ToString();
            }
            return minPoint;
        }
        private void clearAllStripMenuItem_Click(object sender, EventArgs e)
        {
            clearValueTextBoxes();
            clearMinPointTextBoxes();
            clearGraph();
            disableMinControls();
        }
        private void clearGraphtoolStripMenuItem_Click(object sender, EventArgs e)
        {
            clearMinPointTextBoxes();
            clearGraph();
            disableMinControls();
        }
        private void clearValuesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            clearValueTextBoxes();
            clearMinPointTextBoxes();
        }
        private void clearValueTextBoxes()
        {
            textBoxA.Clear();
            textBoxB.Clear();
            textBoxE.Clear();
            textBoxExpression.Clear();
        }
        private void clearMinPointTextBoxes()
        {
            textBoxMinPointX.Clear();
            textBoxMinPointY.Clear();
        }
        private void clearGraph()
        {
            pane.CurveList.Clear();
            zedGraphControl.Refresh();
        }
        private void disableMinControls()
        {
            countMinToolStripMenuItem.Enabled = false;
            buttonStepBack.Enabled = false;
            buttonStepForward.Enabled = false;
        }
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }
        private void checkDoubleInput(TextBox textBox, KeyPressEventArgs e)
        {
            if (e.KeyChar == '.' && textBox.Text.IndexOf('.') != -1)
            {
                e.Handled = true;
                return;
            }
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != 8 && e.KeyChar != '.' && e.KeyChar != '-')
            {
                e.Handled = true;
            }
        }
        private void textBoxA_KeyPress(object sender, KeyPressEventArgs e)
        {
            checkDoubleInput(textBoxA, e);
        }
        private void textBoxB_KeyPress(object sender, KeyPressEventArgs e)
        {
            checkDoubleInput(textBoxB, e);
        }
        private void textBoxE_KeyPress(object sender, KeyPressEventArgs e)
        {
            checkDoubleInput(textBoxE, e);
        }
    }
}
