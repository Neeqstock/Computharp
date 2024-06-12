using ColorHelper;
using Computharp.Behaviors;
using NITHdmis.Modules.Keyboard;
using NITHdmis.Modules.MIDI;
using NITHdmis.Modules.Mouse;
using NITHdmis.Music;
using NITHlibrary.Tools.Filters.ValueFilters;
using RawInputProcessor;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace Computharp.Modules
{
    public class ComputharpDmiBox
    {
        #region Unintegrated values

        private bool bowStaccatos = false; // todo: bug. Sometimes noteOn get stuck with SWAM Viola

        #endregion Unintegrated values

        public List<KeyValuePair<Button, Key>> K_Row0;
        public List<KeyValuePair<Button, Key>> K_Row1;
        public List<KeyValuePair<Button, Key>> K_Row2;
        public List<KeyValuePair<Button, Key>> K_Row3;
        public List<KeyValuePair<Button, Key>> K_Row4;
        public List<KeyValuePair<Button, Key>> K_Fun;
        public List<MidiNotes> NotesOn = new();
        public List<MidiNotes> DroneNotes = new();
        public List<MidiNotes> OctavePedalNotes = new();

        // todo: bring all key assignments to keybindings!
        #region Keybindings     
        public const Key KEYBIND_KB1 = Key.Insert;
        public const Key KEYBIND_KB2 = Key.Delete;
        public const Key KEYBIND_BOW = Key.Oem5;
        #endregion

        private List<KeyValuePair<KKey, AbsNotes>> NumpadScaleSelectors { get; set; } = new();

        private Scale? currentScale = null;
        public int OctavePedalRule { get; set; } = -12;

        private bool octavePedal = false;
        public bool OctavePedal
        {
            get { return octavePedal; }
            set
            {
                // LOGIC: change immediately all notes octave, ignoring drone pedal notes
                octavePedal = value;
                List<MidiNotes> NotesOnCopy = new List<MidiNotes>(NotesOn);

                if (!octavePedal)
                {    
                    foreach(MidiNotes note in NotesOnCopy)
                    {
                        if (!DroneNotes.Contains(note))
                        {
                            NoteOff(note);
                            NotesOn.Remove(note);
                            NoteOn(note - OctavePedalRule);
                            NotesOn.Add(note - OctavePedalRule);
                        }
                    }
                }
                else if (octavePedal)
                {
                    foreach (MidiNotes note in NotesOnCopy)
                    {
                        if (!DroneNotes.Contains(note))
                        {
                            NoteOff(note);
                            NotesOn.Remove(note);
                            NoteOn(note + OctavePedalRule);
                            NotesOn.Add(note + OctavePedalRule);
                        }
                    }
                }
            }
        }
        public Scale? CurrentScale
        {
            get { return currentScale; }
            set
            {
                currentScale = value;
                HighlightScale(currentScale);
            }
        }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        public ComputharpDmiBox(MainWindow window)
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        {
            MW = window;

            CreateKeysLists();
            CreatePrimaryKeyboard();
            CreateSecondaryKeyboard();

            MidiModule = new MidiModuleNAudio(1, 1);

            KeyboardModule = new KeyboardModuleWPF(new WindowInteropHelper(window).Handle, RawInputCaptureMode.Foreground);
            KeyboardModule.KeyboardBehaviors.Add(new KBlistenKeystrokes());

            MouseModule = new MouseModule(20000, 17f, MouseModuleModes.Normal, 150f);
            MouseModule.VelocityFilterX = new DoubleFilterMAExpDecaying(0.1f);
            MouseModule.VelocityFilterY = new DoubleFilterMAExpDecaying(0.1f);
            MouseModule.Behaviors.Add(new MBlistenMouseBow());
            MouseModule.StartPolling();

            NotifyIndicatorsChange();
        }

        public MainWindow MW { get; set; }

        public KeyboardModuleWPF KeyboardModule { get; set; }
        public MouseModule MouseModule { get; set; }
        public MidiModuleNAudio MidiModule { get; set; }

        public Keyboard PrimaryKeyboard { get; set; }
        public Keyboard SecondaryKeyboard { get; set; }

        public int MasterPitch { get; set; } = 60;
        public int HRule { get; set; } = 1;
        public int VRule { get; set; } = 3;
        public bool AltSelector { get; set; } = false;
        public bool DronePedal { get; set; } = false;

        public int ModulationMin { get; set; } = 0;
        public int ModulationMax { get; set; } = 50;

        private bool bowOn = false;

        public bool BowOn
        {
            get { return bowOn; }
            set
            {
                bowOn = value;
                switch (bowOn)
                {
                    case true:
                        Pressure = 0;
                        MouseModule.SetFpsOffsetToCurrentMousePosition();
                        MouseModule.MouseMode = MouseModuleModes.FPS;
                        MouseModule.SetCursorVisible(false);
                        MouseModule.SetCursorVisible(false);
                        MouseModule.SetCursorVisible(false);
                        MouseModule.SetCursorVisible(false);
                        MouseModule.SetCursorVisible(false);
                        break;

                    case false:
                        Pressure = 127;
                        MouseModule.MouseMode = MouseModuleModes.Normal;
                        MouseModule.SetCursorVisible(true);
                        MouseModule.SetCursorVisible(true);
                        MouseModule.SetCursorVisible(true);
                        MouseModule.SetCursorVisible(true);
                        MouseModule.SetCursorVisible(true);
                        break;
                }
                NotifyIndicatorsChange();
            }
        }

        public double BowForce { get; set; } = 1f;

        private List<MidiNotes> tempNotesOn;

        internal void ReceiveBowGesture(double velocityY, bool directionChange)
        {
            if (BowOn)
            {
                if (directionChange && bowStaccatos)
                {
                    tempNotesOn = new List<MidiNotes>(NotesOn);
                    foreach (MidiNotes note in tempNotesOn)
                    {
                        NoteOff(note);
                        NoteOn(note);
                    }
                }
                Pressure = (int)(velocityY * BowForce);
            }
        }

        internal void ReceiveKeyStroke(RawInputEventArgs e)
        {
            MW.lbl_KeyCode.Content = e.Key.ToString();
            MW.lbl_KeyState.Content = e.KeyPressState.ToString();

            // Separated into two cycles with return to give priority to notes
            foreach (List<KKey> kKeys in PrimaryKeyboard.NoteKeys)
            {
                foreach (KKey k in kKeys)
                {
                    if (e.Key.Equals(k.Key))
                    {
                        ProcessKeyStateChange(e, k);
                        return;     // Break free if key was found
                    }
                }
            }
            foreach (KKey k in PrimaryKeyboard.FunKeys)
            {
                if (e.Key.Equals(k.Key))
                {
                    ProcessKeyStateChange(e, k);
                    return;         // Break free if key was found
                }
            }
        }

        private void NotifyIndicatorsChange()
        {
            MW.lbl_MidiOut.Content = MidiModule.OutDevice.ToString();

            if (PrimaryKeyboard.RawDevice != null)
            {
                MW.lbl_PrimaryKeyboardId.Content = PrimaryKeyboard.RawDevice.Name.ToString();
                MW.lbl_PrimaryKeyboardTransp.Content = PrimaryKeyboard.Transp.ToString();
                MW.lbl_PrimaryKeyboardOctave.Content = PrimaryKeyboard.Octave.ToString();

                foreach(var kkey in PrimaryKeyboard.FunKeys)
                {
                    if (kkey.Key == KEYBIND_KB1)
                    {
                        kkey.BaseBackground = Rack.BrushMiddle;
                        kkey.Button.Background = kkey.BaseBackground;
                    }
                }
            }
            else
            {
                MW.lbl_PrimaryKeyboardId.Content = "-";
                MW.lbl_PrimaryKeyboardTransp.Content = "-";
                MW.lbl_PrimaryKeyboardOctave.Content = "-";

                foreach (var kkey in PrimaryKeyboard.FunKeys)
                {
                    if (kkey.Key == KEYBIND_KB1)
                    {
                        kkey.BaseBackground = Rack.BrushNeutral;
                        kkey.Button.Background = kkey.BaseBackground;
                    }
                }
            }

            if (SecondaryKeyboard.RawDevice != null)
            {
                MW.lbl_SecondaryKeyboardId.Content = SecondaryKeyboard.RawDevice.Name.ToString();
                MW.lbl_SecondaryKeyboardTransp.Content = SecondaryKeyboard.Transp.ToString();
                MW.lbl_SecondaryKeyboardOctave.Content = SecondaryKeyboard.Octave.ToString();

                foreach (var kkey in PrimaryKeyboard.FunKeys)
                {
                    if (kkey.Key == KEYBIND_KB2)
                    {
                        kkey.BaseBackground = Rack.BrushMiddle;
                        kkey.Button.Background = kkey.BaseBackground;
                    }
                }
            }
            else
            {
                MW.lbl_SecondaryKeyboardId.Content = "-";
                MW.lbl_SecondaryKeyboardTransp.Content = "-";
                MW.lbl_SecondaryKeyboardOctave.Content = "-";

                foreach (var kkey in PrimaryKeyboard.FunKeys)
                {
                    if (kkey.Key == KEYBIND_KB2)
                    {
                        kkey.BaseBackground = Rack.BrushNeutral;
                        kkey.Button.Background = kkey.BaseBackground;
                    }
                }
            }

            switch (BowOn)
            {
                case true:
                    MW.lbl_BowOn.Content = "On";
                    foreach (var kkey in PrimaryKeyboard.FunKeys)
                    {
                        if (kkey.Key == KEYBIND_BOW)
                        {
                            kkey.BaseBackground = Rack.BrushMiddle;
                            kkey.Button.Background = kkey.BaseBackground;
                        }
                    }
                    break;

                case false:
                    MW.lbl_BowOn.Content = "Off";
                    foreach (var kkey in PrimaryKeyboard.FunKeys)
                    {
                        if (kkey.Key == KEYBIND_BOW)
                        {
                            kkey.BaseBackground = Rack.BrushNeutral;
                            kkey.Button.Background = kkey.BaseBackground;
                        }
                    }
                    break;
            }

            if(CurrentScale != null)
            {
                MW.lbl_CurrentScale.Content = CurrentScale.GetName();
                switch (CurrentScale.ScaleCode)
                {
                    case ScaleCodes.min:
                        MW.lbl_CurrentScale.Foreground = Rack.BrushMinor;
                        break;
                    case ScaleCodes.maj:
                        MW.lbl_CurrentScale.Foreground = Rack.BrushMajor;
                        break;
                    case ScaleCodes.chrom:
                        MW.lbl_CurrentScale.Foreground = Rack.BrushDefaultString;
                        break;
                }
            }
            else
            {
                MW.lbl_CurrentScale.Content = Rack.DEFAULTVOIDSTRING;
                MW.lbl_CurrentScale.Foreground = Rack.BrushDefaultString;
            }

            // Numpad scale color
            if(CurrentScale != null)
            {
                foreach (KeyValuePair<KKey, AbsNotes> numkey in NumpadScaleSelectors)
                {
                    if (numkey.Value == CurrentScale.RootNote)
                    {
                        switch (CurrentScale.ScaleCode)
                        {
                            case ScaleCodes.min:
                                numkey.Key.BaseBackground = Rack.BrushMinor;
                                numkey.Key.Button.Background = numkey.Key.BaseBackground;
                                break;
                            case ScaleCodes.maj:
                                numkey.Key.BaseBackground = Rack.BrushMajor;
                                numkey.Key.Button.Background = numkey.Key.BaseBackground;
                                break;
                            case ScaleCodes.chrom:
                                numkey.Key.BaseBackground = Rack.BrushNeutral;
                                numkey.Key.Button.Background = numkey.Key.BaseBackground;
                                break;
                        }
                    }
                    else
                    {
                        numkey.Key.BaseBackground = Rack.BrushNeutral;
                        numkey.Key.Button.Background = numkey.Key.BaseBackground;
                    }
                }
            }
            else
            {
                foreach(var numkey in NumpadScaleSelectors)
                {
                    numkey.Key.BaseBackground = Rack.BrushNeutral;
                    numkey.Key.Button.Background = numkey.Key.BaseBackground;
                }
            }
        }

        private void CreateKeysLists()
        {
            KeyValuePair<Button, Key> RogueKey = new(MW.b_less, Key.OemBackslash);

            K_Row0 = new List<KeyValuePair<Button, Key>>()
            {
                new(MW.b_z, Key.Z),
                new(MW.b_x, Key.X),
                new(MW.b_c, Key.C),
                new(MW.b_v, Key.V),
                new(MW.b_b, Key.B),
                new(MW.b_n, Key.N),
                new(MW.b_m, Key.M),
                new(MW.b_comma, Key.OemComma),
                new(MW.b_period, Key.OemPeriod),
                new(MW.b_minus, Key.OemMinus)
            };
            K_Row1 = new List<KeyValuePair<Button, Key>>()
            {
                new(MW.b_a, Key.A),
                new(MW.b_s, Key.S),
                new(MW.b_d, Key.D),
                new(MW.b_f, Key.F),
                new(MW.b_g, Key.G),
                new(MW.b_h, Key.H),
                new(MW.b_j, Key.J),
                new(MW.b_k, Key.K),
                new(MW.b_l, Key.L),
                new(MW.b_oacc, Key.Oem3),
                new(MW.b_aacc, Key.OemQuotes),
                new(MW.b_uacc, Key.OemQuestion)
            };
            K_Row2 = new List<KeyValuePair<Button, Key>>()
            {
                new(MW.b_q, Key.Q),
                new(MW.b_w, Key.W),
                new(MW.b_e, Key.E),
                new(MW.b_r, Key.R),
                new(MW.b_t, Key.T),
                new(MW.b_y, Key.Y),
                new(MW.b_u, Key.U),
                new(MW.b_i, Key.I),
                new(MW.b_o, Key.O),
                new(MW.b_p, Key.P),
                new(MW.b_eacc, Key.Oem1),
                new(MW.b_plus, Key.OemPlus),
            };
            K_Row3 = new List<KeyValuePair<Button, Key>>()
            {
                new(MW.b_1, Key.D1),
                new(MW.b_2, Key.D2),
                new(MW.b_3, Key.D3),
                new(MW.b_4, Key.D4),
                new(MW.b_5, Key.D5),
                new(MW.b_6, Key.D6),
                new(MW.b_7, Key.D7),
                new(MW.b_8, Key.D8),
                new(MW.b_9, Key.D9),
                new(MW.b_0, Key.D0),
                new(MW.b_accent, Key.OemOpenBrackets),
                new(MW.b_iacc, Key.Oem6)
            };
            K_Row4 = new List<KeyValuePair<Button, Key>>()
            {
                new(MW.b_f1, Key.F1),
                new(MW.b_f2, Key.F2),
                new(MW.b_f3, Key.F3),
                new(MW.b_f4, Key.F4),
                new(MW.b_f5, Key.F5),
                new(MW.b_f6, Key.F6),
                new(MW.b_f7, Key.F7),
                new(MW.b_f8, Key.F8),
                new(MW.b_f9, Key.F9),
                new(MW.b_f10, Key.F10),
                new(MW.b_f11, Key.F11),
                new(MW.b_f12, Key.F12),
            };
            K_Fun = new List<KeyValuePair<Button, Key>>()
            {
                new(MW.b_lctrl, Key.LeftCtrl),
                new(MW.b_win, Key.LWin),
                new(MW.b_alt, Key.LeftAlt),
                new(MW.b_space, Key.Space),
                new(MW.b_altgr, Key.RightAlt),
                new(MW.b_menu, Key.Apps),
                new(MW.b_rctrl, Key.RightCtrl),

                new(MW.b_left, Key.Left),
                new(MW.b_up, Key.Up),
                new(MW.b_right, Key.Right),
                new(MW.b_down, Key.Down),

                new(MW.b_lshift, Key.LeftShift),
                new(MW.b_less, Key.OemBackslash),
                new(MW.b_rshift, Key.RightShift),
                new(MW.b_caps, Key.CapsLock),
                new(MW.b_tab, Key.Tab),
                new(MW.b_enter, Key.Return),
                new(MW.b_backslash, Key.Oem5),
                new(MW.b_backcanc, Key.Back),
                new(MW.b_esc, Key.Escape),
                new(MW.b_stamp, Key.Snapshot),
                new(MW.b_ins, Key.Insert),
                new(MW.b_canc, Key.Delete),
                //new KeyValuePair<Button, Key>(MW.b_numenter, Key.Return), // Problem: same keycode as non-numpad one
                new(MW.b_numplus, Key.Add),
                new(MW.b_blocnum, Key.NumLock),
                new(MW.b_numminus, Key.Subtract),
                new(MW.b_home, Key.Home),
                new(MW.b_fine, Key.End),
                new(MW.b_pgup, Key.PageUp),
                new(MW.b_pgdn, Key.Next),

                new(MW.b_num0, Key.NumPad0),
                new(MW.b_numperiod, Key.Decimal),
                new(MW.b_num1, Key.NumPad1),
                new(MW.b_num2, Key.NumPad2),
                new(MW.b_num3, Key.NumPad3),
                new(MW.b_num4, Key.NumPad4),
                new(MW.b_num5, Key.NumPad5),
                new(MW.b_num6, Key.NumPad6),
                new(MW.b_num7, Key.NumPad7),
                new(MW.b_num8, Key.NumPad8),
                new(MW.b_num9, Key.NumPad9),
                new(MW.b_frontslash, Key.Divide),
                new(MW.b_asterisk, Key.Multiply),
            };
        }

        private void CreatePrimaryKeyboard()
        {
            PrimaryKeyboard = new Keyboard();
            PrimaryKeyboard.NoteKeys.Add(new List<KKey>());
            PrimaryKeyboard.NoteKeys.Add(new List<KKey>());
            PrimaryKeyboard.NoteKeys.Add(new List<KKey>());
            PrimaryKeyboard.NoteKeys.Add(new List<KKey>());
            PrimaryKeyboard.NoteKeys.Add(new List<KKey>());
            PrimaryKeyboard.FunKeys = new List<KKey>();
            int i;

            // Row 0
            i = MasterPitch;
            foreach (KeyValuePair<Button, Key> kvp in K_Row0)
            {
                PrimaryKeyboard.NoteKeys[0].Add(new KKey() { Button = kvp.Key, Key = kvp.Value, KKeyType = KKeyTypes.Note, MidiNote = (MidiNotes)i });
                i += HRule;
            }

            // Row 1
            i = MasterPitch + VRule;
            foreach (KeyValuePair<Button, Key> kvp in K_Row1)
            {
                PrimaryKeyboard.NoteKeys[1].Add(new KKey() { Button = kvp.Key, Key = kvp.Value, KKeyType = KKeyTypes.Note, MidiNote = (MidiNotes)i });
                i += HRule;
            }

            // Row 2
            i = MasterPitch + VRule * 2;
            foreach (KeyValuePair<Button, Key> kvp in K_Row2)
            {
                PrimaryKeyboard.NoteKeys[2].Add(new KKey() { Button = kvp.Key, Key = kvp.Value, KKeyType = KKeyTypes.Note, MidiNote = (MidiNotes)i });
                i += HRule;
            }

            // Row 3
            i = MasterPitch + VRule * 3;
            foreach (KeyValuePair<Button, Key> kvp in K_Row3)
            {
                PrimaryKeyboard.NoteKeys[3].Add(new KKey() { Button = kvp.Key, Key = kvp.Value, KKeyType = KKeyTypes.Note, MidiNote = (MidiNotes)i });
                i += HRule;
            }

            // Row 4
            i = MasterPitch + VRule * 4;
            foreach (KeyValuePair<Button, Key> kvp in K_Row4)
            {
                PrimaryKeyboard.NoteKeys[4].Add(new KKey() { Button = kvp.Key, Key = kvp.Value, KKeyType = KKeyTypes.Note, MidiNote = (MidiNotes)i });
                i += HRule;
            }

            // Fun
            foreach (KeyValuePair<Button, Key> kvp in K_Fun)
            {
                PrimaryKeyboard.FunKeys.Add(new KKey() { Button = kvp.Key, Key = kvp.Value, KKeyType = KKeyTypes.Functional, MidiNote = MidiNotes.NaN });
                i += HRule;
            }

            // Numpad Scale Selectors list
            NumpadScaleSelectors.Clear();
            foreach (KKey kkey in PrimaryKeyboard.FunKeys)
            {
                if (kkey.Key == Key.NumPad0) { NumpadScaleSelectors.Add(new KeyValuePair<KKey, AbsNotes>(kkey, AbsNotes.C)); }
                if (kkey.Key == Key.Decimal) { NumpadScaleSelectors.Add(new KeyValuePair<KKey, AbsNotes>(kkey, AbsNotes.sC)); }
                if (kkey.Key == Key.NumPad1) { NumpadScaleSelectors.Add(new KeyValuePair<KKey, AbsNotes>(kkey, AbsNotes.D)); }
                if (kkey.Key == Key.NumPad2) { NumpadScaleSelectors.Add(new KeyValuePair<KKey, AbsNotes>(kkey, AbsNotes.sD)); }
                if (kkey.Key == Key.NumPad3) { NumpadScaleSelectors.Add(new KeyValuePair<KKey, AbsNotes>(kkey, AbsNotes.E)); }
                if (kkey.Key == Key.NumPad4) { NumpadScaleSelectors.Add(new KeyValuePair<KKey, AbsNotes>(kkey, AbsNotes.F)); }
                if (kkey.Key == Key.NumPad5) { NumpadScaleSelectors.Add(new KeyValuePair<KKey, AbsNotes>(kkey, AbsNotes.sF)); }
                if (kkey.Key == Key.NumPad6) { NumpadScaleSelectors.Add(new KeyValuePair<KKey, AbsNotes>(kkey, AbsNotes.G)); }
                if (kkey.Key == Key.NumPad7) { NumpadScaleSelectors.Add(new KeyValuePair<KKey, AbsNotes>(kkey, AbsNotes.sG)); }
                if (kkey.Key == Key.NumPad8) { NumpadScaleSelectors.Add(new KeyValuePair<KKey, AbsNotes>(kkey, AbsNotes.A)); }
                if (kkey.Key == Key.NumPad9) { NumpadScaleSelectors.Add(new KeyValuePair<KKey, AbsNotes>(kkey, AbsNotes.sA)); }
                if (kkey.Key == Key.Divide) { NumpadScaleSelectors.Add(new KeyValuePair<KKey, AbsNotes>(kkey, AbsNotes.B)); }
                if (kkey.Key == Key.Multiply) { NumpadScaleSelectors.Add(new KeyValuePair<KKey, AbsNotes>(kkey, AbsNotes.NaN)); }
            }

        }

        private void CreateSecondaryKeyboard()
        {
            SecondaryKeyboard = new Keyboard();
            SecondaryKeyboard.NoteKeys.Add(new List<KKey>());
            SecondaryKeyboard.NoteKeys.Add(new List<KKey>());
            SecondaryKeyboard.NoteKeys.Add(new List<KKey>());
            SecondaryKeyboard.NoteKeys.Add(new List<KKey>());
            SecondaryKeyboard.NoteKeys.Add(new List<KKey>());
            SecondaryKeyboard.FunKeys = new List<KKey>();
            int i;

            // Row 1
            i = MasterPitch;
            foreach (KeyValuePair<Button, Key> kvp in K_Row0)
            {
                SecondaryKeyboard.NoteKeys[0].Add(new KKey() { Button = kvp.Key, Key = kvp.Value, KKeyType = KKeyTypes.Note, MidiNote = (MidiNotes)i });
                i += HRule;
            }

            // Row 2
            i = MasterPitch + VRule;
            foreach (KeyValuePair<Button, Key> kvp in K_Row1)
            {
                SecondaryKeyboard.NoteKeys[1].Add(new KKey() { Button = kvp.Key, Key = kvp.Value, KKeyType = KKeyTypes.Note, MidiNote = (MidiNotes)i });
                i += HRule;
            }

            // Row 3
            i = MasterPitch + VRule * 2;
            foreach (KeyValuePair<Button, Key> kvp in K_Row2)
            {
                SecondaryKeyboard.NoteKeys[2].Add(new KKey() { Button = kvp.Key, Key = kvp.Value, KKeyType = KKeyTypes.Note, MidiNote = (MidiNotes)i });
                i += HRule;
            }

            // Row 4
            i = MasterPitch + VRule * 3;
            foreach (KeyValuePair<Button, Key> kvp in K_Row3)
            {
                SecondaryKeyboard.NoteKeys[3].Add(new KKey() { Button = kvp.Key, Key = kvp.Value, KKeyType = KKeyTypes.Note, MidiNote = (MidiNotes)i });
                i += HRule;
            }

            // Row 5
            i = MasterPitch + VRule * 4;
            foreach (KeyValuePair<Button, Key> kvp in K_Row4)
            {
                SecondaryKeyboard.NoteKeys[4].Add(new KKey() { Button = kvp.Key, Key = kvp.Value, KKeyType = KKeyTypes.Note, MidiNote = (MidiNotes)i });
                i += HRule;
            }

            // Fun
            foreach (KeyValuePair<Button, Key> kvp in K_Fun)
            {
                SecondaryKeyboard.FunKeys.Add(new KKey() { Button = kvp.Key, Key = kvp.Value, KKeyType = KKeyTypes.Functional, MidiNote = MidiNotes.NaN });
                i += HRule;
            }
        }

        private void ProcessKeyStateChange(RawInputEventArgs rawEvent, KKey kkey)
        {
            // Keyboard Identification
            Keyboards kb = Keyboards.NaK;
            if (PrimaryKeyboard.RawDevice != null)
            {
                if (rawEvent.Device.Name == PrimaryKeyboard.RawDevice.Name)
                {
                    kb = Keyboards.Primary;
                }
            }
            if (SecondaryKeyboard.RawDevice != null)
            {
                if (rawEvent.Device.Name == SecondaryKeyboard.RawDevice.Name)
                {
                    kb = Keyboards.Secondary;
                }
            }

            // Background setting
            switch (rawEvent.KeyPressState)
            {
                case KeyPressState.Up:
                    kkey.Button.Background = kkey.BaseBackground;
                    break;

                case KeyPressState.Down:
                    SetActiveBackgroundGivenKeyboard(kkey, kb);
                    break;
            }

            // Event rerouting. Note keys send to note change, function keys OR note keys + alt. modifier send to functions
            if (kkey.KKeyType == KKeyTypes.Note && !AltSelector)
            {
                ProcessNoteKeyChange(rawEvent, kkey, kb);
            }
            else if (kkey.KKeyType == KKeyTypes.Functional || (kkey.KKeyType == KKeyTypes.Note && AltSelector))
            {
                ProcessFunctionKeyChange(rawEvent, kkey, kb);
            }
        }

        private void ProcessFunctionKeyChange(RawInputEventArgs rawEvent, KKey kkey, Keyboards kb)
        {
            if (rawEvent.KeyPressState != kkey.KeyPressState)
            {
                kkey.KeyPressState = rawEvent.KeyPressState;

                if (rawEvent.KeyPressState == KeyPressState.Down) // Key Down
                {
                    if (!AltSelector) // Alt selector off
                    {
                        switch (rawEvent.Key)
                        {
                            // Midi Out selection
                            case Key.PageUp:
                                MidiModule.OutDevice++;
                                NotifyIndicatorsChange();
                                break;

                            case Key.PageDown:
                                MidiModule.OutDevice--;
                                NotifyIndicatorsChange();
                                break;

                            // Scales Highlighting
                            case Key.NumPad0:
                                ProcessNumPadScaleChange(Key.NumPad0);
                                break;

                            case Key.Decimal:
                                ProcessNumPadScaleChange(Key.Decimal);
                                break;

                            case Key.NumPad1:
                                ProcessNumPadScaleChange(Key.NumPad1);
                                break;

                            case Key.NumPad2:
                                ProcessNumPadScaleChange(Key.NumPad2);
                                break;

                            case Key.NumPad3:
                                ProcessNumPadScaleChange(Key.NumPad3);
                                break;

                            case Key.NumPad4:
                                ProcessNumPadScaleChange(Key.NumPad4);
                                break;

                            case Key.NumPad5:
                                ProcessNumPadScaleChange(Key.NumPad5);
                                break;

                            case Key.NumPad6:
                                ProcessNumPadScaleChange(Key.NumPad6);
                                break;

                            case Key.NumPad7:
                                ProcessNumPadScaleChange(Key.NumPad7);
                                break;

                            case Key.NumPad8:
                                ProcessNumPadScaleChange(Key.NumPad8);
                                break;

                            case Key.NumPad9:
                                ProcessNumPadScaleChange(Key.NumPad9);
                                break;

                            case Key.Divide:
                                ProcessNumPadScaleChange(Key.Divide);
                                break;

                            case Key.Multiply:
                                ProcessNumPadScaleChange(Key.Multiply);
                                break;

                            // Alt selector
                            case Key.Return:
                                AltSelector = true;
                                break;

                            // Modulator key
                            case Key.Back:
                                ProcessModulator(true);
                                break;

                            // MainKeyboard selector
                            case KEYBIND_KB2:
                                SetPrimaryKeyboard(rawEvent.Device);
                                break;
                            // SecondaryKeyboard selector
                            case KEYBIND_KB1:
                                SetSecondaryKeyboard(rawEvent.Device);
                                break;

                            // Octave scroll
                            case Key.Up:
                                if (rawEvent.Device == PrimaryKeyboard.RawDevice)
                                {
                                    PrimaryKeyboard.Octave++;
                                }
                                if (rawEvent.Device == SecondaryKeyboard.RawDevice)
                                {
                                    SecondaryKeyboard.Octave++;
                                }
                                NotifyIndicatorsChange();
                                break;

                            case Key.Down:
                                if (rawEvent.Device == PrimaryKeyboard.RawDevice)
                                {
                                    PrimaryKeyboard.Octave--;
                                }
                                if (rawEvent.Device == SecondaryKeyboard.RawDevice)
                                {
                                    SecondaryKeyboard.Octave--;
                                }
                                NotifyIndicatorsChange();
                                break;

                            // Transpose
                            case Key.Right:
                                if (rawEvent.Device == PrimaryKeyboard.RawDevice)
                                {
                                    PrimaryKeyboard.Transp++;
                                }
                                if (rawEvent.Device == SecondaryKeyboard.RawDevice)
                                {
                                    SecondaryKeyboard.Transp++;
                                }
                                NotifyIndicatorsChange();
                                break;

                            case Key.Left:
                                if (rawEvent.Device == PrimaryKeyboard.RawDevice)
                                {
                                    PrimaryKeyboard.Transp--;
                                }
                                if (rawEvent.Device == SecondaryKeyboard.RawDevice)
                                {
                                    SecondaryKeyboard.Transp--;
                                }
                                NotifyIndicatorsChange();
                                break;

                            // Bow on/off
                            case KEYBIND_BOW:
                                if (BowOn)
                                {
                                    BowOn = false;
                                }
                                else
                                {
                                    BowOn = true;
                                }
                                break;

                            // Octave pedal
                            case Key.Space:
                                if(!OctavePedal)
                                    OctavePedal = true;
                                break;

                            default:
                                break;
                        }
                    }
                    else if (AltSelector)   // Alt selector on
                    {
                        switch (rawEvent.Key)
                        {
                            case Key.D1:
                                HRule = 1;
                                VRule = 1;
                                ProcessHVRuleChange();
                                break;
                            case Key.D2:
                                HRule = 1;
                                VRule = 2;
                                ProcessHVRuleChange();
                                break;
                            case Key.D3:
                                HRule = 1;
                                VRule = 3;
                                ProcessHVRuleChange();
                                break;
                            case Key.D4:
                                HRule = 1;
                                VRule = 4;
                                ProcessHVRuleChange();
                                break;
                            case Key.D5:
                                HRule = 1;
                                VRule = 5;
                                ProcessHVRuleChange();
                                break;
                            case Key.D6:
                                HRule = 1;
                                VRule = 6;
                                ProcessHVRuleChange();
                                break;

                            default: 
                                break;
                        }
                    }

                }
                else if (rawEvent.KeyPressState == KeyPressState.Up) // Key Up
                {
                    switch (rawEvent.Key)
                    {
                        // Alt selector
                        case Key.Return:
                            AltSelector = false;
                            break;

                        // Modulator key
                        case Key.Back:
                            ProcessModulator(false);
                            break;

                        // Octave pedal
                        case Key.Space:
                            if(OctavePedal)
                                OctavePedal = false;
                            break;

                        // Drone pedal
                        case Key.LeftCtrl:
                            ProcessDronePedal();
                            break;

                        default:
                            break;
                    }
                }

                if(kkey.KKeyType == KKeyTypes.Note && rawEvent.KeyPressState == KeyPressState.Down)
                {
                    AbsNotes newRoot = kkey.MidiNote.ToAbsNote();
                    CurrentScaleFuzzySelect(newRoot);

                    NotifyIndicatorsChange();
                }
            }
        }

        private void CurrentScaleFuzzySelect(AbsNotes newRoot)
        {
            if(newRoot == AbsNotes.NaN)
            {
                CurrentScale = null;
            }
            else if (CurrentScale != null)
            {
                AbsNotes currentRoot = CurrentScale.RootNote;

                if (currentRoot != newRoot)
                {
                    CurrentScale = new Scale(newRoot, ScaleCodes.maj);
                }
                else
                {
                    switch (CurrentScale.ScaleCode)
                    {
                        case ScaleCodes.maj:
                            CurrentScale = new Scale(newRoot, ScaleCodes.min);
                            break;
                        case ScaleCodes.min:
                            CurrentScale = new Scale(newRoot, ScaleCodes.maj);
                            break;
                    }
                }
            }
            else
            {
                CurrentScale = CurrentScale = new Scale(newRoot, ScaleCodes.maj);
            }
        }

        private void ProcessNumPadScaleChange(Key keyToProcess)
        {
            AbsNotes rootNote = AbsNotes.NaN;

            foreach(var numkey in NumpadScaleSelectors)
            {
                if (numkey.Key.Key == keyToProcess)
                {
                    rootNote = numkey.Value;
                }
            }

            CurrentScaleFuzzySelect(rootNote);

        }

        private void ProcessHVRuleChange()
        {
            int f;

            // Primary Keyboard

            // Row 0
            f = MasterPitch;
            foreach (KKey key in PrimaryKeyboard.NoteKeys[0])
            {
                key.MidiNote = (MidiNotes)f;
                f += HRule;
            }

            // Row 1
            f = MasterPitch + VRule;
            foreach (KKey key in PrimaryKeyboard.NoteKeys[1])
            {
                key.MidiNote = (MidiNotes)f;
                f += HRule;
            }

            // Row 2
            f = MasterPitch + VRule * 2;
            foreach (KKey key in PrimaryKeyboard.NoteKeys[2])
            {
                key.MidiNote = (MidiNotes)f;
                f += HRule;
            }

            // Row 3
            f = MasterPitch + VRule * 3;
            foreach (KKey key in PrimaryKeyboard.NoteKeys[3])
            {
                key.MidiNote = (MidiNotes)f;
                f += HRule;
            }

            // Row 4
            f = MasterPitch + VRule * 4;
            foreach (KKey key in PrimaryKeyboard.NoteKeys[4])
            {
                key.MidiNote = (MidiNotes)f;
                f += HRule;
            }

            // Secondary Keyboard

            // Row 0
            f = MasterPitch;
            foreach (KKey key in PrimaryKeyboard.NoteKeys[0])
            {
                key.MidiNote = (MidiNotes)f;
                f += HRule;
            }

            // Row 1
            f = MasterPitch + VRule;
            foreach (KKey key in SecondaryKeyboard.NoteKeys[1])
            {
                key.MidiNote = (MidiNotes)f;
                f += HRule;
            }

            // Row 2
            f = MasterPitch + VRule * 2;
            foreach (KKey key in SecondaryKeyboard.NoteKeys[2])
            {
                key.MidiNote = (MidiNotes)f;
                f += HRule;
            }

            // Row 3
            f = MasterPitch + VRule * 3;
            foreach (KKey key in SecondaryKeyboard.NoteKeys[3])
            {
                key.MidiNote = (MidiNotes)f;
                f += HRule;
            }

            // Row 4
            f = MasterPitch + VRule * 4;
            foreach (KKey key in SecondaryKeyboard.NoteKeys[4])
            {
                key.MidiNote = (MidiNotes)f;
                f += HRule;
            }

            HighlightScale(CurrentScale);
        }

        private void ProcessNoteKeyChange(RawInputEventArgs rawEvent, KKey kkey, Keyboards kb)
        {
            MidiNotes noteToPlay = MidiNotes.NaN;

            // Determine note to play given keyboard
            switch (kb)
            {
                case Keyboards.Primary:
                    noteToPlay = kkey.MidiNote + PrimaryKeyboard.Transp + 12 * PrimaryKeyboard.Octave;
                    break;

                case Keyboards.Secondary:
                    noteToPlay = kkey.MidiNote + SecondaryKeyboard.Transp + 12 * SecondaryKeyboard.Octave;
                    break;

                case Keyboards.NaK:
                    noteToPlay = MidiNotes.NaN;
                    break;
            }

            // Add octave pedal rule if octave pedal is down
            if (OctavePedal)
            {
                noteToPlay += OctavePedalRule;
            }

            // Process note on/off
            if (rawEvent.KeyPressState == KeyPressState.Up)
            {
                if (NotesOn.Contains(noteToPlay) && !DroneNotes.Contains(noteToPlay))
                {
                    NotesOn.Remove(noteToPlay);
                    NoteOff(noteToPlay);
                }
            }
            else if (rawEvent.KeyPressState == KeyPressState.Down)
            {
                if (!NotesOn.Contains(noteToPlay))
                {
                    NotesOn.Add(noteToPlay);
                    NoteOn(noteToPlay);
                }
            }

        }

        private static void SetActiveBackgroundGivenKeyboard(KKey kkey, Keyboards kb)
        {
            switch (kb)
            {
                case Keyboards.Primary:
                    kkey.Button.Background = Rack.BrushActivePrimary;
                    break;

                case Keyboards.Secondary:
                    kkey.Button.Background = Rack.BrushActiveSecondary;
                    break;

                case Keyboards.NaK:
                    break;
            }
        }

        private void SetPrimaryKeyboard(RawKeyboardDevice device)
        {
            SecondaryKeyboard.RawDevice = device;
            if (PrimaryKeyboard.RawDevice == device)
            {
                PrimaryKeyboard.RawDevice = null;
            }
            NotifyIndicatorsChange();
        }

        private void SetSecondaryKeyboard(RawKeyboardDevice device)
        {
            PrimaryKeyboard.RawDevice = device;
            if (SecondaryKeyboard.RawDevice == device)
            {
                SecondaryKeyboard.RawDevice = null;
            }
            NotifyIndicatorsChange();
        }

        private void ProcessModulator(bool v)
        {
            switch (v)
            {
                case true:
                    MidiModule.SetModulation(ModulationMax);
                    break;

                case false:
                    MidiModule.SetModulation(ModulationMin);
                    break;
            }
        }

        private void ProcessDronePedal()
        {
            foreach (MidiNotes note in DroneNotes)
            {
                NoteOff(note);
                NotesOn.Remove(note);
            }
            DroneNotes.Clear();

            foreach (MidiNotes note in NotesOn)
            {
                DroneNotes.Add(note);
            }

            // Highlight
            foreach (List<KKey> lst in PrimaryKeyboard.NoteKeys)
            {
                foreach (KKey key in lst)
                {
                    bool drone = false;
                    foreach (MidiNotes note in DroneNotes)
                    {
                        if (key.MidiNote.ToAbsNote() == note.ToAbsNote())
                        {
                            drone = true;
                        }
                    }
                    if (drone)
                    {
                        key.Button.BorderThickness = new System.Windows.Thickness(2);
                    }
                    else
                    {
                        key.Button.BorderThickness = new System.Windows.Thickness(0);
                    }
                }
            }
        }

        private void HighlightScale(Scale? scale)
        {
            foreach (List<KKey> lst in PrimaryKeyboard.NoteKeys)
            {
                foreach (KKey k in lst)
                {
                    if (scale != null)
                    {
                        if (scale.IsInScale(k.MidiNote))
                        {
                            RGB clr = ColorHelper.ColorConverter.HslToRgb(Rack.ScaleColors[scale.NotesInScale.IndexOf(k.MidiNote.ToAbsNote())]);
                            k.BaseBackground = new SolidColorBrush(Color.FromArgb(Rack.KEYBASEALPHA, clr.R, clr.G, clr.B));
                            k.Button.Background = k.BaseBackground;
                        }
                        else
                        {
                            k.BaseBackground = Rack.BrushNeutral;
                            k.Button.Background = k.BaseBackground;
                        }
                    }
                    else
                    {
                        k.BaseBackground = Rack.BrushNeutral;
                        k.Button.Background = k.BaseBackground;
                    }

                    // Drones
                    foreach (MidiNotes droneNote in DroneNotes)
                    {
                        if (droneNote.ToAbsNote() == k.MidiNote.ToAbsNote())
                        {
                            //k.BaseBackground = Rack.BrushDrone;
                            //k.Button.Background = Rack.BrushDrone;
                            //if(k.KeyPressState == KeyPressState.Down)
                            //{
                            //    k.Button.Background = Rack.BrushActivePrimary;
                            //}
                        }
                    }
                }
            }

            NotifyIndicatorsChange();
        }

        private void NoteOn(MidiNotes midiNote)
        {
            MidiModule.PlayNote((int)midiNote, 127);
        }

        private void NoteOff(MidiNotes midiNote)
        {
            MidiModule.StopNote((int)midiNote);
        }

        private int pressure = 127;

        public int Pressure
        {
            get
            {
                return pressure;
            }
            set
            {
                if (value > 127)
                {
                    pressure = 127;
                }
                else if (value < 0)
                {
                    pressure = 0;
                }
                else
                {
                    pressure = value;
                }

                MidiModule.SetPressure(pressure);
            }
        }
    }
}