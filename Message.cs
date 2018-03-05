﻿/* ----------------------------------------------------------------------------
Transonic MIDI Library
Copyright (C) 1995-2018  George E Greaney

This program is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
as published by the Free Software Foundation; either version 2
of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program; if not, write to the Free Software
Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
----------------------------------------------------------------------------*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//J Glatt's Midi page: http://midi.teragonaudio.com/tech/midispec.htm

namespace Transonic.MIDI
{
    public class Message
    {

//- static methods ------------------------------------------------------------

//- base class ----------------------------------------------------------------

        public Message()
        {
        }

        //for splitting a midi msg - handles subclass fields too
        public Message copy()
        {
            return (Message)this.MemberwiseClone();
        }

        //for sending a msg to an output device
        virtual public byte[] getDataBytes() 
        {
            return null;
        }
    }

//- subclasses ----------------------------------------------------------------

//-----------------------------------------------------------------------------
//  CHANNEL MESSAGES
//-----------------------------------------------------------------------------

    //channel message base class
    public class ChannelMessage : Message
    {
        public int channel;

        public ChannelMessage(int _channel) : base()
        {
            channel = _channel;
        }
    }

    public class NoteOnMessage : ChannelMessage     //0x90
    {
        public int noteNumber;
        public int velocity;

        public NoteOnMessage(int channel, int note, int vel)
            : base(channel)
        {
            noteNumber = note;
            velocity = vel;
        }

        public NoteOnMessage(MidiInStream stream, int channel)
            : base(channel)
        {
            noteNumber = stream.getOne();
            velocity = stream.getOne();
        }

        override public byte[] getDataBytes()
        {
            byte[] bytes = new byte[3];
            bytes[0] = (byte)(0x90 + channel);
            bytes[1] = (byte)noteNumber;
            bytes[2] = (byte)velocity;
            return bytes;
        }

        public override string ToString()
        {
            return "Note On (" + channel + ") note = " + noteNumber + ", velocity = " + velocity;
        }
    }

    public class NoteOffMessage : ChannelMessage   //0x80
    {
        public int noteNumber;
        public int velocity;

        public NoteOffMessage(int channel, int note, int vel)
            : base(channel)
        {
            noteNumber = note;
            velocity = vel;
        }

        public NoteOffMessage(MidiInStream stream, int channel)
            : base(channel)
        {
            noteNumber = stream.getOne();
            velocity = stream.getOne();
        }

        override public byte[] getDataBytes()
        {
            byte[] bytes = new byte[3];
            bytes[0] = (byte)(0x80 + channel);
            bytes[1] = (byte)noteNumber;
            bytes[2] = (byte)velocity;
            return bytes;
        }

        public override string ToString()
        {
            return "Note Off (" + channel + ") note = " + noteNumber;
        }
    }

    public class AftertouchMessage : ChannelMessage     //0xA0
    {
        public int noteNumber;
        public int pressure;

        public AftertouchMessage(int channel, int note, int press)
            : base(channel)
        {
            noteNumber = note;
            pressure = press;
        }

        public AftertouchMessage(MidiInStream stream, int channel)
            : base(channel)
        {
            noteNumber = stream.getOne();
            pressure = stream.getOne();
        }

        override public byte[] getDataBytes()
        {
            byte[] bytes = new byte[3];
            bytes[0] = (byte)(0xa0 + channel);
            bytes[1] = (byte)noteNumber;
            bytes[2] = (byte)pressure;
            return bytes;
        }
    }

    public class ControllerMessage : ChannelMessage     //0xB0
    {
        public int ctrlNumber;
        public int ctrlValue;

        public ControllerMessage(int channel, int num, int val)
            : base(channel)
        {
            ctrlNumber = num;
            ctrlValue = val;
        }

        public ControllerMessage(MidiInStream stream, int channel)
            : base(channel)
        {
            ctrlNumber = stream.getOne();
            ctrlValue = stream.getOne();
        }

        override public byte[] getDataBytes()
        {
            byte[] bytes = new byte[3];
            bytes[0] = (byte)(0xb0 + channel);
            bytes[1] = (byte)ctrlNumber;
            bytes[2] = (byte)ctrlValue;
            return bytes;
        }

        public override string ToString()
        {
            return "Controller (" + channel + ") number = " + ctrlNumber + ", value = " + ctrlValue;
        }
    }

    public class PatchChangeMessage : ChannelMessage       //0xC0
    {
        public int patchNumber;

        public PatchChangeMessage(int channel, int num)
            : base(channel)
        {
            patchNumber = num;
        }

        public PatchChangeMessage(MidiInStream stream, int channel)
            : base(channel)
        {
            patchNumber = stream.getOne();
        }

        override public byte[] getDataBytes()
        {
            byte[] bytes = new byte[2];
            bytes[0] = (byte)(0xc0 + channel);
            bytes[1] = (byte)patchNumber;
            return bytes;
        }

        public override string ToString()
        {
            return "Patch Change (" + channel + ") number = " + patchNumber;
        }
    }

    public class ChannelPressureMessage : ChannelMessage       //0xD0
    {
        public int pressure;

        public ChannelPressureMessage(int channel, int press)
            : base(channel)
        {
            pressure = press;
        }

        public ChannelPressureMessage(MidiInStream stream, int channel)
            : base(channel)
        {
            pressure = stream.getOne();
        }

        override public byte[] getDataBytes()
        {
            byte[] bytes = new byte[2];
            bytes[0] = (byte)(0xd0 + channel);
            bytes[1] = (byte)pressure;
            return bytes;
        }
    }

    public class PitchWheelMessage : ChannelMessage     //0xE0
    {
        public int wheel;

        public PitchWheelMessage(int channel, int _wheel)
            : base(channel)
        {
            wheel = _wheel;
        }

        public PitchWheelMessage(MidiInStream stream, int channel)
            : base(channel)
        {
            int b1 = stream.getOne();
            int b2 = stream.getOne();
            wheel = b1 * 128 + b2;
        }

        override public byte[] getDataBytes()
        {
            byte[] bytes = new byte[3];
            bytes[0] = (byte)(0xe0 + channel);
            bytes[1] = (byte)(wheel / 128);
            bytes[2] = (byte)(wheel % 128);
            return bytes;
        }
    }

//-----------------------------------------------------------------------------
//  SYSTEM MESSAGES
//-----------------------------------------------------------------------------

    public class SysExMessage : Message
    {
        List<int> sysExData;

        public SysExMessage(MidiInStream stream)
            : base()
        {
            sysExData = new List<int>();
            int b1 = stream.getOne();
            while (b1 != 0xf7)
            {
                sysExData.Add(b1);
                b1 = stream.getOne();
            }            
        }

        override public byte[] getDataBytes()
        {
            byte[] bytes = new byte[sysExData.Count];
            for (int i = 0; i < sysExData.Count; i++)
            {
                bytes[i] = (byte)sysExData[i];
            }
            return bytes;
        }
    }

    public enum SYSTEMMESSAGE { 
        QUARTERFRAME = 0Xf1,        //f1
        SONGPOSITION,               //f2
        SONGSELECT,                 //f3
        UNDEFINED1,                 //f4
        UNDEFINED2,                 //f5
        TUNEREQUEST,                //f6
        SYSEXEND,                   //f7
        MIDICLOCK,                  //f8
        MIDITICK,                   //f9
        MIDISTART,                  //fa
        MIDICONTINUE,               //fb
        MIDISTOP,                   //fc
        UNDEFINED3,                 //fd
        ACTIVESENSE = 0xfe          //fe
    }; 

    public class SystemMessage : Message
    {
        SYSTEMMESSAGE msgtype;
        int value;

        public SystemMessage(MidiInStream stream, int status)
            : base()
        {
            msgtype = (SYSTEMMESSAGE)status;
            value = 0;
            switch (msgtype)
            {
                case SYSTEMMESSAGE.QUARTERFRAME :
                case SYSTEMMESSAGE.SONGSELECT :
                    value = stream.getOne();
                    break;
                case SYSTEMMESSAGE.SONGPOSITION:
                    int b1 = stream.getOne();
                    int b2 = stream.getOne();
                    value = b1 * 128 + b2;
                    break;
                default :
                    break;
            }        
        }

        int[] SysMsgLen = {1, 2, 3, 2, 0, 0, 1, 1, 1, 1, 1, 1, 1, 0, 1, 0};

        override public byte[] getDataBytes()
        {
            byte[] bytes = new byte[SysMsgLen[(byte)msgtype - 0xF0]];
            bytes[0] = (byte)msgtype;
            switch (msgtype)
            {
                case SYSTEMMESSAGE.QUARTERFRAME:
                case SYSTEMMESSAGE.SONGSELECT:
                    bytes[1] = (byte)value;
                    break;
                case SYSTEMMESSAGE.SONGPOSITION:
                    bytes[1] = (byte)(value / 128);
                    bytes[2] = (byte)(value % 128);
                    break;
                default:
                    break;
            }
            return bytes;
        }
    }
}

//Console.WriteLine("there's no sun in the shadow of the wizard");
