﻿#region Copyright & License Information
/*
 * Copyright 2007-2010 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made 
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation. For more information,
 * see LICENSE.
 */
#endregion

using System.Collections.Generic;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using SZipFile = ICSharpCode.SharpZipLib.Zip.ZipFile;

namespace OpenRA.FileFormats
{
	public class ZipFile : IFolder
	{
		readonly SZipFile pkg;
		int priority;

		public ZipFile(string filename, int priority)
		{
			this.priority = priority;
			if (File.Exists(filename))
			{
				try
				{
					pkg = new SZipFile(File.OpenRead(filename));
				}
				catch (ZipException e)
				{
					Log.Write("debug", "Couldn't load zip file: {0}", e.Message);
				}
			}
			else
				pkg = SZipFile.Create(filename);
		}

		public Stream GetContent(string filename)
		{
			return pkg.GetInputStream(pkg.GetEntry(filename));
		}

		public IEnumerable<uint> AllFileHashes()
		{
			foreach(ZipEntry entry in pkg)
				yield return PackageEntry.HashFilename(entry.Name);
		}
		
		public bool Exists(string filename)
		{
			return pkg.GetEntry(filename) != null;
		}

		public int Priority
		{
			get { return 500 + priority; }
		}
		
		public void Write(Dictionary<string, byte[]> contents)
		{
			pkg.BeginUpdate();
			// TODO: Clear existing content?
			
			foreach (var kvp in contents)
			{
				pkg.Add(new StaticMemoryDataSource(kvp.Value), kvp.Key);
			}
			pkg.CommitUpdate();
		}
	}

	class StaticMemoryDataSource : IStaticDataSource
	{
		byte[] data;
		public StaticMemoryDataSource (byte[] data)
		{
			this.data = data;
		}
		
		public Stream GetSource()
		{
			return new MemoryStream(data);
		}
	}
}
