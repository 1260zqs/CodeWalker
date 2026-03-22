using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeWalker.Utils;

public partial class ProgressForm : Form
{
    public ProgressForm()
    {
        InitializeComponent();
    }

    public static ProgressForm Create(string title, CancellationTokenSource cts)
    {
        var form = new ProgressForm();
        form.Text = title;
        form.cts = cts;
        return form;
    }

    private int currentValue = 0;
    private CancellationTokenSource cts;

    public void SetMaxValue(int max)
    {
        if (!IsHandleCreated) return;

        BeginInvoke(() =>
        {
            currentValue = 0;
            progressBar1.Value = 0;
            progressBar1.Maximum = max;
        });
    }

    public void IncreaseValue(string infoText)
    {
        if (!IsHandleCreated) return;

        BeginInvoke(() =>
        {
            statusText.Text = infoText;
            progressBar1.Value = ++currentValue;
        });
    }

    public void ClearProgress()
    {
        cts = null;
        if (InvokeRequired)
        {
            BeginInvoke(Close);
            return;
        }
        Close();
    }

    public void UpdateStatusTex(string infoText)
    {
        if (!IsHandleCreated) return;
        BeginInvoke(() =>
        {
            statusText.Text = infoText;
        });
    }

    public void UpdateProgress(string infoText, float value)
    {
        if (!IsHandleCreated) return;

        BeginInvoke(() =>
        {
            statusText.Text = infoText;
            progressBar1.Maximum = 100;
            progressBar1.Value = Mathf.FloorToInt(value * 100);
        });
    }

    public void UpdateProgress(string infoText, int value, int maxValue)
    {
        if (!IsHandleCreated) return;

        BeginInvoke(() =>
        {
            statusText.Text = infoText;
            progressBar1.Maximum = maxValue;
            progressBar1.Value = value;
        });
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);
        if (cts != null && e.CloseReason == CloseReason.UserClosing)
        {
            e.Cancel = true;
            cts.Cancel();
            cts = null;
        }
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);
    }

    private void cancelBtn_Click(object sender, EventArgs e)
    {
        cancelBtn.Enabled = false;
        cts?.Cancel();
        cts = null;
    }
}