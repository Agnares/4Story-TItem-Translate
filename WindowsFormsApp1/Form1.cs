using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace WindowsFormsApp1
{
	public partial class Form1 : Form
	{
		List<OpenFileDialog> lOfd = new List<OpenFileDialog>();
		List<SaveFileDialog> lSfd = new List<SaveFileDialog>();

		List<List<TItemStruct>> loadedTcds = new List<List<TItemStruct>>();

		public class CBinaryReader : BinaryReader
		{
			public CBinaryReader(Stream input) : base(input)
			{
			}

			public CBinaryReader(Stream input, Encoding encoding) : base(input, encoding)
			{
			}

			public CBinaryReader(Stream input, Encoding encoding, bool leaveOpen) : base(input, encoding, leaveOpen)
			{
			}

			public string ReadCString()
			{
				ushort length = base.ReadByte();
				if (length >= byte.MaxValue)
					length = base.ReadUInt16();
				return Encoding.Default.GetString(base.ReadBytes(length));
			}
		}

		public class CBinaryWriter : BinaryWriter
		{
			public CBinaryWriter(Stream input) : base(input)
			{
			}

			public CBinaryWriter(Stream input, Encoding encoding) : base(input, encoding)
			{
			}

			public CBinaryWriter(Stream input, Encoding encoding, bool leaveOpen) : base(input, encoding, leaveOpen)
			{
			}

			public void WriteUInt16Length(int length)
			{
				base.Write(byte.MaxValue);
				base.Write((ushort)length);
			}

			public void WriteByteLength(int length)
			{
				base.Write((byte)length);
			}

			public void WriteCString(string text)
			{
				int length = text.Length;
				if (length >= byte.MaxValue)
					WriteUInt16Length(length);
				else
					WriteByteLength(length);
				base.Write(Encoding.Default.GetBytes(text));
			}
		}

		public struct TItemStruct
		{
			// Lists of Not Used
			public List<byte> lByteNotUsed;
			public List<uint> lUInt32NotUsed;

			// Lists of Used
			public List<ushort> lVisual;
			public List<ushort> lOptionSFX;

			public ushort	wItemID			{ get; set; }
			public byte		bType			{ get; set; }
			public byte		bKind			{ get; set; }
			public ushort	wAttrID			{ get; set; }
			public string	strNAME			{ get; set; }
			public ushort	wUseValue		{ get; set; }
			public uint		dwSlotID		{ get; set; }
			public uint		dwClassID		{ get; set; }
			public byte		bPrmSlotID		{ get; set; }
			public byte		bSubSlotID		{ get; set; }
			public byte		bLevel			{ get; set; }
			public byte		bCanRepair		{ get; set; }
			public uint		dwDuraMax		{ get; set; }
			public byte		bRefineMax		{ get; set; }
			public float	fPriceRate		{ get; set; }
			public uint		dwPrice			{ get; set; }
			public byte		bMinRange		{ get; set; }
			public byte		bMaxRange		{ get; set; }
			public byte		bStack			{ get; set; }
			public byte		bSlotCount		{ get; set; }
			public byte		bCanGamble		{ get; set; }
			public byte		bGambleProb		{ get; set; }
			public byte		bDestoryProb	{ get; set; }
			public byte		bCanGrade		{ get; set; }
			public byte		bCanMagic		{ get; set; }
			public byte		bCanRare		{ get; set; }
			public ushort	wDelayGroupID	{ get; set; }
			public uint		dwDelay			{ get; set; }
			public byte		bCanTrade		{ get; set; }
			public byte		bIsSpecial		{ get; set; }
			public ushort	wUseTime		{ get; set; }
			public byte		bUseType		{ get; set; }
			public byte		bWeaponID		{ get; set; }
			public float	fShotSpeed		{ get; set; }
			public float	fGravity		{ get; set; }
			public uint		dwInfoID		{ get; set; }
			public byte		bSkillItemType	{ get; set; }
			public ushort	wGradeSFX		{ get; set; }
			public byte		bCanWrap		{ get; set; }
			public uint		dwAuctionCode	{ get; set; }
			public byte		bCanColor		{ get; set; }
			public ushort	wUseBP			{ get; set; }
		}

		public Form1()
        {
            InitializeComponent();
        }

        private void textBox1_DoubleClick(object sender, EventArgs e)
        {
			OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Tcd files (TItem.tcd)|TItem.tcd";
            ofd.FilterIndex = 2;
            ofd.RestoreDirectory = true;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                textBox1.ReadOnly = true;
                textBox1.Text = Path.GetFileName(ofd.FileName);

                lOfd.Add(ofd);
            }
        }

        private void textBox2_DoubleClick(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
			ofd.Filter = "Tcd files (TItem.tcd)|TItem.tcd";
			ofd.FilterIndex = 2;
            ofd.RestoreDirectory = true;

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                textBox2.ReadOnly = true;
                textBox2.Text = Path.GetFileName(ofd.FileName);

                lOfd.Add(ofd);
            }
        }

        private void textBox3_DoubleClick(object sender, EventArgs e)
        {
			SaveFileDialog sfd = new SaveFileDialog();
			sfd.Filter = "All files (*.*)|*.*";
			sfd.FilterIndex = 2;
			sfd.RestoreDirectory = true;

			if (sfd.ShowDialog() == DialogResult.OK)
			{
				textBox3.ReadOnly = true;
				textBox3.Text = Path.GetFileName(sfd.FileName);

				lSfd.Add(sfd);
			}
		}

        private void button1_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < lOfd.Count; i++)
            {
                ReadTItem(lOfd[i].OpenFile());   
            }

			for (int i = 0; i < lSfd.Count; i++)
			{
				WriteTItem(lSfd[i].OpenFile());
			}
        }

		void ReadTItem(Stream stream)
        {
			CBinaryReader br = new CBinaryReader(stream);
			List<TItemStruct> loadedTcd = new List<TItemStruct>();

			ushort wCount = br.ReadUInt16();
			for (ushort i = 0; i < wCount; i++)
			{
				TItemStruct sItem = new TItemStruct();

				// Lists of Not Used
				sItem.lByteNotUsed = new List<byte>();
				sItem.lUInt32NotUsed = new List<uint>();

				// Lists of Used
				sItem.lVisual = new List<ushort>();
				sItem.lOptionSFX = new List<ushort>();

				sItem.wItemID			= br.ReadUInt16();
				sItem.bType				= br.ReadByte();
				sItem.bKind				= br.ReadByte();
				sItem.wAttrID			= br.ReadUInt16();
				sItem.strNAME			= br.ReadCString();
				sItem.wUseValue			= br.ReadUInt16();
				sItem.dwSlotID			= br.ReadUInt32();
				sItem.dwClassID			= br.ReadUInt32();
				sItem.bPrmSlotID		= br.ReadByte();
				sItem.bSubSlotID		= br.ReadByte();
				sItem.bLevel			= br.ReadByte();
				sItem.bCanRepair		= br.ReadByte();
				sItem.dwDuraMax			= br.ReadUInt32();
				sItem.bRefineMax		= br.ReadByte();
				sItem.fPriceRate		= br.ReadSingle();
				sItem.dwPrice			= br.ReadUInt32();
				sItem.bMinRange			= br.ReadByte();
				sItem.bMaxRange			= br.ReadByte();
				sItem.bStack			= br.ReadByte();
				sItem.bSlotCount		= br.ReadByte();
				sItem.bCanGamble		= br.ReadByte();
				sItem.bGambleProb		= br.ReadByte();
				sItem.bDestoryProb		= br.ReadByte();

				sItem.lByteNotUsed.Add(br.ReadByte());
				sItem.lByteNotUsed.Add(br.ReadByte());

				sItem.bCanGrade			= br.ReadByte();
				sItem.bCanMagic			= br.ReadByte();
				sItem.bCanRare			= br.ReadByte();
				sItem.wDelayGroupID		= br.ReadUInt16();
				sItem.dwDelay			= br.ReadUInt32();
				sItem.bCanTrade			= br.ReadByte();

				sItem.lByteNotUsed.Add(br.ReadByte());

				sItem.bIsSpecial		= br.ReadByte();
				sItem.wUseTime			= br.ReadUInt16();
				sItem.bUseType			= br.ReadByte();
				sItem.bWeaponID			= br.ReadByte();
				sItem.fShotSpeed		= br.ReadSingle();
				sItem.fGravity			= br.ReadSingle();
				sItem.dwInfoID			= br.ReadUInt32();
				sItem.bSkillItemType	= br.ReadByte();

				sItem.lVisual.Add(br.ReadUInt16());
				sItem.lVisual.Add(br.ReadUInt16());
				sItem.lVisual.Add(br.ReadUInt16());
				sItem.lVisual.Add(br.ReadUInt16());
				sItem.lVisual.Add(br.ReadUInt16());

				sItem.wGradeSFX			= br.ReadUInt16();

				sItem.lOptionSFX.Add(br.ReadUInt16());
				sItem.lOptionSFX.Add(br.ReadUInt16());
				sItem.lOptionSFX.Add(br.ReadUInt16());

				sItem.bCanWrap			= br.ReadByte();
				sItem.dwAuctionCode		= br.ReadUInt32();
				sItem.bCanColor			= br.ReadByte();

				sItem.lUInt32NotUsed.Add(br.ReadUInt32());
				sItem.lByteNotUsed.Add(br.ReadByte());

				sItem.wUseBP			= br.ReadUInt16();

				sItem.lByteNotUsed.Add(br.ReadByte());
				sItem.lByteNotUsed.Add(br.ReadByte());
				sItem.lUInt32NotUsed.Add(br.ReadUInt32());
				sItem.lByteNotUsed.Add(br.ReadByte());


				loadedTcd.Add(sItem);
			}

			loadedTcds.Add(loadedTcd);

			br.Close();
        }

		void WriteTItem(Stream stream)
		{
			CBinaryWriter bw = new CBinaryWriter(stream);

			if (loadedTcds.Count > 1)
			{
				List<TItemStruct> lItemsFirst = new List<TItemStruct>(loadedTcds[0]);
				List<TItemStruct> lItemsSecond = new List<TItemStruct>(loadedTcds[1]);

				ushort nTotalItemsFirst = (ushort)lItemsFirst.Count;
				ushort nTotalItemsSecond = (ushort)lItemsSecond.Count;

				bw.Write(nTotalItemsFirst);

				for (ushort i = 0; i < nTotalItemsFirst; i++)
				{
					string strNAME = lItemsFirst[i].strNAME;
					for (ushort j = 0; j < nTotalItemsSecond; j++)
					{
						if (lItemsFirst[i].wItemID == lItemsSecond[j].wItemID)
							strNAME = lItemsSecond[j].strNAME;
					}
					bw.Write(lItemsFirst[i].wItemID);
					bw.Write(lItemsFirst[i].bType);
					bw.Write(lItemsFirst[i].bKind);
					bw.Write(lItemsFirst[i].wAttrID);
					bw.WriteCString(strNAME);
					bw.Write(lItemsFirst[i].wUseValue);
					bw.Write(lItemsFirst[i].dwSlotID);
					bw.Write(lItemsFirst[i].dwClassID);
					bw.Write(lItemsFirst[i].bPrmSlotID);
					bw.Write(lItemsFirst[i].bSubSlotID);
					bw.Write(lItemsFirst[i].bLevel);
					bw.Write(lItemsFirst[i].bCanRepair);
					bw.Write(lItemsFirst[i].dwDuraMax);
					bw.Write(lItemsFirst[i].bRefineMax);
					bw.Write(lItemsFirst[i].fPriceRate);
					bw.Write(lItemsFirst[i].dwPrice);
					bw.Write(lItemsFirst[i].bMinRange);
					bw.Write(lItemsFirst[i].bMaxRange);
					bw.Write(lItemsFirst[i].bStack);
					bw.Write(lItemsFirst[i].bSlotCount);
					bw.Write(lItemsFirst[i].bCanGamble);
					bw.Write(lItemsFirst[i].bGambleProb);
					bw.Write(lItemsFirst[i].bDestoryProb);

					bw.Write(lItemsFirst[i].lByteNotUsed[0]);
					bw.Write(lItemsFirst[i].lByteNotUsed[1]);

					bw.Write(lItemsFirst[i].bCanGrade);
					bw.Write(lItemsFirst[i].bCanMagic);
					bw.Write(lItemsFirst[i].bCanRare);
					bw.Write(lItemsFirst[i].wDelayGroupID);
					bw.Write(lItemsFirst[i].dwDelay);
					bw.Write(lItemsFirst[i].bCanTrade);

					bw.Write(lItemsFirst[i].lByteNotUsed[2]);

					bw.Write(lItemsFirst[i].bIsSpecial);
					bw.Write(lItemsFirst[i].wUseTime);
					bw.Write(lItemsFirst[i].bUseType);
					bw.Write(lItemsFirst[i].bWeaponID);
					bw.Write(lItemsFirst[i].fShotSpeed);
					bw.Write(lItemsFirst[i].fGravity);
					bw.Write(lItemsFirst[i].dwInfoID);
					bw.Write(lItemsFirst[i].bSkillItemType);

					bw.Write(lItemsFirst[i].lVisual[0]);
					bw.Write(lItemsFirst[i].lVisual[1]);
					bw.Write(lItemsFirst[i].lVisual[2]);
					bw.Write(lItemsFirst[i].lVisual[3]);
					bw.Write(lItemsFirst[i].lVisual[4]);

					bw.Write(lItemsFirst[i].wGradeSFX);

					bw.Write(lItemsFirst[i].lOptionSFX[0]);
					bw.Write(lItemsFirst[i].lOptionSFX[1]);
					bw.Write(lItemsFirst[i].lOptionSFX[2]);

					bw.Write(lItemsFirst[i].bCanWrap);
					bw.Write(lItemsFirst[i].dwAuctionCode);
					bw.Write(lItemsFirst[i].bCanColor);

					bw.Write(lItemsFirst[i].lUInt32NotUsed[0]);
					bw.Write(lItemsFirst[i].lByteNotUsed[3]);

					bw.Write(lItemsFirst[i].wUseBP);

					bw.Write(lItemsFirst[i].lByteNotUsed[4]);
					bw.Write(lItemsFirst[i].lByteNotUsed[5]);
					bw.Write(lItemsFirst[i].lUInt32NotUsed[1]);
					bw.Write(lItemsFirst[i].lByteNotUsed[6]);
				}
			}

			bw.Close();
			MessageBox.Show("TCD Translated");
		}
    }
}
