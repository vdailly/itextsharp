/*
    This file is part of the iText (R) project.
    Copyright (c) 1998-2019 iText Group NV
    Authors: iText Software.

This program is free software; you can redistribute it and/or modify it under the terms of the GNU Affero General Public License version 3 as published by the Free Software Foundation with the addition of the following permission added to Section 15 as permitted in Section 7(a): FOR ANY PART OF THE COVERED WORK IN WHICH THE COPYRIGHT IS OWNED BY iText Group NV, iText Group NV DISCLAIMS THE WARRANTY OF NON INFRINGEMENT OF THIRD PARTY RIGHTS.

This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Affero General Public License for more details.
You should have received a copy of the GNU Affero General Public License along with this program; if not, see http://www.gnu.org/licenses or write to the Free Software Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA, 02110-1301 USA, or download the license from the following URL:

http://itextpdf.com/terms-of-use/

The interactive user interfaces in modified source and object code versions of this program must display Appropriate Legal Notices, as required under Section 5 of the GNU Affero General Public License.

In accordance with Section 7(b) of the GNU Affero General Public License, a covered work must retain the producer line in every PDF that is created or manipulated using iText.

You can be released from the requirements of the license by purchasing a commercial license. Buying such a license is mandatory as soon as you develop commercial activities involving the iText software without disclosing the source code of your own applications.
These activities include: offering paid services to customers as an ASP, serving PDFs on the fly in a web application, shipping iText with a closed source product.

For more information, please contact iText Software Corp. at this address: sales@itextpdf.com */
using System;
using System.IO;
using Org.BouncyCastle.Bcpg.Sig;
using Org.BouncyCastle.Utilities.IO;

namespace Org.BouncyCastle.Bcpg
{
	/**
	* reader for signature sub-packets
	*/
	public class SignatureSubpacketsParser
	{
		private readonly Stream input;

		public SignatureSubpacketsParser(
			Stream input)
		{
			this.input = input;
		}

		public SignatureSubpacket ReadPacket()
		{
			int l = input.ReadByte();
			if (l < 0)
				return null;

			int bodyLen = 0;
			if (l < 192)
			{
				bodyLen = l;
			}
			else if (l <= 223)
			{
				bodyLen = ((l - 192) << 8) + (input.ReadByte()) + 192;
			}
			else if (l == 255)
			{
				bodyLen = (input.ReadByte() << 24) | (input.ReadByte() << 16)
					|  (input.ReadByte() << 8)  | input.ReadByte();
			}
			else
			{
				// TODO Error?
			}

			int tag = input.ReadByte();
			if (tag < 0)
				throw new EndOfStreamException("unexpected EOF reading signature sub packet");

			byte[] data = new byte[bodyLen - 1];
			if (Streams.ReadFully(input, data) < data.Length)
				throw new EndOfStreamException();

			bool isCritical = ((tag & 0x80) != 0);
			SignatureSubpacketTag type = (SignatureSubpacketTag)(tag & 0x7f);
			switch (type)
			{
				case SignatureSubpacketTag.CreationTime:
					return new SignatureCreationTime(isCritical, data);
				case SignatureSubpacketTag.KeyExpireTime:
					return new KeyExpirationTime(isCritical, data);
				case SignatureSubpacketTag.ExpireTime:
					return new SignatureExpirationTime(isCritical, data);
				case SignatureSubpacketTag.Revocable:
					return new Revocable(isCritical, data);
				case SignatureSubpacketTag.Exportable:
					return new Exportable(isCritical, data);
				case SignatureSubpacketTag.IssuerKeyId:
					return new IssuerKeyId(isCritical, data);
				case SignatureSubpacketTag.TrustSig:
					return new TrustSignature(isCritical, data);
				case SignatureSubpacketTag.PreferredCompressionAlgorithms:
				case SignatureSubpacketTag.PreferredHashAlgorithms:
				case SignatureSubpacketTag.PreferredSymmetricAlgorithms:
					return new PreferredAlgorithms(type, isCritical, data);
				case SignatureSubpacketTag.KeyFlags:
					return new KeyFlags(isCritical, data);
				case SignatureSubpacketTag.PrimaryUserId:
					return new PrimaryUserId(isCritical, data);
				case SignatureSubpacketTag.SignerUserId:
					return new SignerUserId(isCritical, data);
				case SignatureSubpacketTag.NotationData:
					return new NotationData(isCritical, data);
			}
			return new SignatureSubpacket(type, isCritical, data);
		}
	}
}
