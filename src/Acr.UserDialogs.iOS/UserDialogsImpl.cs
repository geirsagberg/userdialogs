using System;
using System.Linq;
using BigTed;
using CoreGraphics;
using UIKit;


namespace Acr.UserDialogs {

    public class UserDialogsImpl : AbstractUserDialogs {

        public override void Alert(AlertConfig config) {
            UIApplication.SharedApplication.InvokeOnMainThread(() => {
                if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0)) {
                    var alert = UIAlertController.Create(config.Title ?? String.Empty, config.Message, UIAlertControllerStyle.Alert);
                    alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, x => {
                        if (config.OnOk != null)
                            config.OnOk();
                    }));
                    this.Present(alert);
                }
                else {
                    var dlg = new UIAlertView(config.Title ?? String.Empty, config.Message, null, null, config.OkText);
                    if (config.OnOk != null) 
                        dlg.Clicked += (s, e) => config.OnOk();

                    dlg.Show();
                }
            });
        }


        public override void ActionSheet(ActionSheetConfig config) {
            if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0)) {
                var sheet = UIAlertController.Create(config.Title ?? String.Empty, String.Empty, UIAlertControllerStyle.ActionSheet);
                config.Options.ToList().ForEach(x => 
                    sheet.AddAction(UIAlertAction.Create(x.Text, UIAlertActionStyle.Default, y => {
                        if (x.Action != null)
                            x.Action();
                    }))
                );
                this.Present(sheet);
            }
            else {
                var view = Utils.GetTopView();

                var action = new UIActionSheet(config.Title);
                config.Options.ToList().ForEach(x => action.AddButton(x.Text));

                action.Dismissed += (sender, btn) => {
                    if ((int)btn.ButtonIndex > -1 && (int)btn.ButtonIndex < config.Options.Count)
                        config.Options[(int)btn.ButtonIndex].Action();
                };
                action.ShowInView(view);
            }
        }


        public override void Confirm(ConfirmConfig config) {
            UIApplication.SharedApplication.InvokeOnMainThread(() => {
                if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0)) {
                    var dlg = UIAlertController.Create(config.Title ?? String.Empty, config.Message, UIAlertControllerStyle.Alert);
                    dlg.AddAction(UIAlertAction.Create(config.OkText, UIAlertActionStyle.Default, x => config.OnConfirm(true)));
                    dlg.AddAction(UIAlertAction.Create(config.CancelText, UIAlertActionStyle.Default, x => config.OnConfirm(false)));
                    this.Present(dlg);
                }
                else {
                    var dlg = new UIAlertView(config.Title ?? String.Empty, config.Message, null, config.CancelText, config.OkText);
                    dlg.Clicked += (s, e) => {
                        var ok = ((int)dlg.CancelButtonIndex != (int)e.ButtonIndex);
                        config.OnConfirm(ok);
                    };
                    dlg.Show();
                }
            });
        }


        public override void Login(LoginConfig config) {
            UITextField txtUser = null;
            UITextField txtPass = null;

            UIApplication.SharedApplication.InvokeOnMainThread(() => {

                if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0)) {
                    var dlg = UIAlertController.Create(config.Title ?? String.Empty, config.Message, UIAlertControllerStyle.Alert);
                    dlg.AddAction(UIAlertAction.Create(config.OkText, UIAlertActionStyle.Default, x => config.OnResult(new LoginResult(txtUser.Text, txtPass.Text, true))));
                    dlg.AddAction(UIAlertAction.Create(config.CancelText, UIAlertActionStyle.Default, x => config.OnResult(new LoginResult(txtUser.Text, txtPass.Text, true))));

                    dlg.AddTextField(x => {
                        txtUser = x;
                        x.Placeholder = config.LoginPlaceholder;
                        x.Text = config.LoginValue ?? String.Empty;
                    });
                    dlg.AddTextField(x => {
                        txtPass = x;
                        x.Placeholder = config.PasswordPlaceholder;
                        x.SecureTextEntry = true;
                    });
                    this.Present(dlg);
                }
                else {
                    var dlg = new UIAlertView { AlertViewStyle = UIAlertViewStyle.LoginAndPasswordInput };
                    txtUser = dlg.GetTextField(0);
                    txtPass = dlg.GetTextField(1);

                    txtUser.Placeholder = config.LoginPlaceholder;
                    txtUser.Text = config.LoginValue ?? String.Empty;
                    txtPass.Placeholder = config.PasswordPlaceholder;

                    dlg.Clicked += (s, e) => {
                        var ok = ((int)dlg.CancelButtonIndex != (int)e.ButtonIndex);
                        config.OnResult(new LoginResult(txtUser.Text, txtPass.Text, ok));
                    };
                    dlg.Show();
                }
            });
        }


        public override void Prompt(PromptConfig config) {
            UIApplication.SharedApplication.InvokeOnMainThread(() => {
                var result = new PromptResult();

                if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0)) {
                    var dlg = UIAlertController.Create(config.Title ?? String.Empty, config.Message, UIAlertControllerStyle.Alert);
                    UITextField txt = null;

                    dlg.AddAction(UIAlertAction.Create(config.OkText, UIAlertActionStyle.Default, x => {
                        result.Ok = true;
                        result.Text = txt.Text.Trim();
                        config.OnResult(result);
                    }));
                    dlg.AddAction(UIAlertAction.Create(config.CancelText, UIAlertActionStyle.Default, x => {
                        result.Ok = false;
                        result.Text = txt.Text.Trim();
                        config.OnResult(result);
                    }));
                    dlg.AddTextField(x => {
                        Utils.SetInputType(x, config.InputType);
                        x.Placeholder = config.Placeholder ?? String.Empty;
                        txt = x;
                    });
                    this.Present(dlg);
                }
                else {
                    var isPassword = config.InputType == InputType.Password;

                    var dlg = new UIAlertView(config.Title ?? String.Empty, config.Message, null, config.CancelText, config.OkText) {
                        AlertViewStyle = isPassword
                            ? UIAlertViewStyle.SecureTextInput
                            : UIAlertViewStyle.PlainTextInput
                    };
                    var txt = dlg.GetTextField(0);
                    Utils.SetInputType(txt, config.InputType);
                    txt.Placeholder = config.Placeholder;

                    dlg.Clicked += (s, e) => {
                        result.Ok = ((int)dlg.CancelButtonIndex != (int)e.ButtonIndex);
                        result.Text = txt.Text.Trim();
                        config.OnResult(result);
                    };
                    dlg.Show();
                }
            });
        }


        public override void Toast(string message, int timeoutSeconds = 3, Action onClick = null) {
            UIApplication.SharedApplication.InvokeOnMainThread(() =>  {
                var ms = timeoutSeconds * 1000;
                BTProgressHUD.ShowToast(message, false, ms);
            });
        }


        protected override IProgressDialog CreateDialogInstance() {
            return new ProgressDialog();
        }


        protected virtual void Present(UIAlertController controller) {
            UIApplication.SharedApplication.InvokeOnMainThread(() => {
                var top = Utils.GetTopViewController();
                var po = controller.PopoverPresentationController;
                if (po != null) {
                    po.SourceView = top.View;
                    var h = (top.View.Frame.Height / 2) - 400;
                    var v = (top.View.Frame.Width / 2) - 300;
                    po.SourceRect = new CGRect(v, h, 0, 0);
                    po.PermittedArrowDirections = UIPopoverArrowDirection.Any;
                }
                top.PresentViewController(controller, true, null);
            });
        }


        protected override IProgressIndicator CreateNetworkIndicator() {
            return new NetworkIndicator();
        }
    }
}

        //public override void DateTimePrompt(DateTimePromptConfig config) {
        //    var sheet = new ActionSheetDatePicker {
        //        Title = config.Title,
        //        DoneText = config.OkText
        //    };

        //    switch (config.SelectionType) {
        //        case DateTimeSelectionType.Date:
        //            sheet.DatePicker.Mode = UIDatePickerMode.Date;
        //            break;

        //        case DateTimeSelectionType.Time:
        //            sheet.DatePicker.Mode = UIDatePickerMode.Time;
        //            break;

        //        case DateTimeSelectionType.DateTime:
        //            sheet.DatePicker.Mode = UIDatePickerMode.DateAndTime;
        //            break;
        //    }
        //    if (config.MinValue != null)
        //        sheet.DatePicker.MinimumDate = config.MinValue.Value;

        //    if (config.MaxValue != null)
        //        sheet.DatePicker.MaximumDate = config.MaxValue.Value;

        //    sheet.DateTimeSelected += (sender, args) => {
        //        // TODO: stop adjusting date/time
        //        config.OnResult(new DateTimePromptResult(sheet.DatePicker.Date));
        //    };

        //    var top = Utils.GetTopView();
        //    sheet.Show(top);
        //    //sheet.DatePicker.MinuteInterval
        //}


        //public override void DurationPrompt(DurationPromptConfig config) {
        //    var sheet = new ActionSheetDatePicker {
        //        Title = config.Title,
        //        DoneText = config.OkText
        //    };
        //    sheet.DatePicker.Mode = UIDatePickerMode.CountDownTimer;

        //    sheet.DateTimeSelected += (sender, args) => config.OnResult(new DurationPromptResult(args.TimeOfDay));

        //    var top = Utils.GetTopView();
        //    sheet.Show(top);
        //}