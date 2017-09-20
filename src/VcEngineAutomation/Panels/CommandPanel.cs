﻿using System;
using System.Linq;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.AutomationElements.Infrastructure;
using FlaUI.Core.Conditions;
using FlaUI.Core.Definitions;
using FlaUI.Core.Tools;
using VcEngineAutomation.Extensions;

namespace VcEngineAutomation.Panels
{
    public class CommandPanel : IDisposable
    {
        private readonly VcEngine vcEngine;
        private readonly Lazy<string> lazyTitle; 

        public CommandPanel(VcEngine vcEngine, Func<AutomationElement> paneRetriever)
        {
            this.vcEngine = vcEngine;
            AutomationElement mainPane = paneRetriever();
            if (mainPane == null) throw new InvalidOperationException("No command panel could be found");
            Pane = mainPane.FindFirstDescendant(cf => cf.ByClassName("CommandPanelView"));
            lazyTitle = new Lazy<string>(() => mainPane.Properties.Name.ValueOrDefault);
        }

        public AutomationElement Get(Func<ConditionFactory, ConditionBase> newConditionFunc) 
        {
            return CustomPane.FindFirstDescendant(newConditionFunc);
        }

        public AutomationElement Pane { get; set; }
        public string Title => lazyTitle.Value;

        public AutomationElement CustomPane
        {
            get { return Pane.FindFirstChild(cf => cf.ByControlType(ControlType.Custom)); }
        }

        public Button FindButton(string automationId)
        {
            var element = CustomPane.FindFirstDescendant(cf => cf.ByAutomationId(automationId));
            if (element == null) throw new InvalidOperationException($"Could not find button with automationId='{automationId}'");
            return element.AsButton();
        }

        public TextBox FindTextBox(string automationId)
        {
            var element = CustomPane.FindFirstDescendant(cf => cf.ByAutomationId(automationId));
            if (element == null) throw new InvalidOperationException($"Could not find text box with automationId='{automationId}'");
            return element.AsTextBox();
        }
        public CheckBox FindCheckBox(string automationId)
        {
            var element = CustomPane.FindFirstDescendant(cf => cf.ByAutomationId(automationId));
            if (element == null) throw new InvalidOperationException($"Could not find check box with automationId='{automationId}'");
            return element.AsCheckBox();
        }
        public ComboBox FindComboBox(string automationId)
        {
            var element = CustomPane.FindFirstDescendant(cf => cf.ByAutomationId(automationId));
            if (element == null) throw new InvalidOperationException($"Could not find combo box with automationId='{automationId}'");
            return element.AsComboBox();
        }
        public RadioButton FindRadioButton(string automationId)
        {
            var element = CustomPane.FindFirstDescendant(cf => cf.ByAutomationId(automationId));
            if (element == null) throw new InvalidOperationException($"Could not find radio button with automationId='{automationId}'");
            return element.AsRadioButton();
        }
        public Label FindLabel(string automationId)
        {
            var element = CustomPane.FindFirstDescendant(cf => cf.ByAutomationId(automationId));
            if (element == null) throw new InvalidOperationException($"Could not find label with automationId='{automationId}'");
            return element.AsLabel();
        }
        public Grid FindGrid(string automationId)
        {
            var element = CustomPane.FindFirstDescendant(cf => cf.ByAutomationId(automationId));
            if (element == null) throw new InvalidOperationException($"Could not find grid with automationId='{automationId}'");
            return element.AsGrid();
        }
        public TextBox GetTextBoxForLabel(string label)
        {
            return Pane.GetTextBoxForLabel(label);
        }

        public void Apply(TimeSpan? waitTimeSpan=null)
        {
            vcEngine.WaitWhileBusy();
            Button[] buttons = Pane.FindAllChildren(cf => cf.ByControlType(ControlType.Button)).Select(ae => ae.AsButton()).ToArray();
            if (buttons.Length != 2) throw new InvalidOperationException("Cancel and Apply buttons were not found in command panel");
            Button button = buttons[0];
            Retry.While(() => !button.Properties.IsEnabled.Value, TimeSpan.FromSeconds(5), TimeSpan.FromMilliseconds(200));
            if (button.Properties.IsOffscreen) throw new InvalidOperationException("Apply button was not visible");
            if (!button.Properties.IsEnabled) throw new InvalidOperationException("Apply button was not enabled");
            button.Invoke();
            vcEngine.WaitWhileBusy(waitTimeSpan);
        }

        public void Cancel(TimeSpan? waitTimeSpan=null)
        {
            vcEngine.WaitWhileBusy();
            Button[] buttons = Pane.FindAllChildren(cf => cf.ByControlType(ControlType.Button)).Select(ae => ae.AsButton()).ToArray();
            if (buttons.Length != 2) throw new InvalidOperationException("Cancel and Apply buttons were not found in command panel");
            Button button = buttons[1];
            Retry.While(() => !button.Properties.IsEnabled.Value, TimeSpan.FromSeconds(5), TimeSpan.FromMilliseconds(200));
            if (button.Properties.IsOffscreen) throw new InvalidOperationException("Cancel button was not visible");
            if (!button.Properties.IsEnabled) throw new InvalidOperationException("Cancel button was not enabled");
            button.Invoke();
            vcEngine.WaitWhileBusy(waitTimeSpan);
        }

        public void Close(TimeSpan? waitTimeSpan = null)
        {
            Cancel(waitTimeSpan);
        }

        public void Dispose()
        {
            if (Pane.IsVisible())
            {
                Close();
            }
        }
    }
}
