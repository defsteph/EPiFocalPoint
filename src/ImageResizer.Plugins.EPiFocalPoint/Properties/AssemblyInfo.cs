using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

using EPiServer.PlugIn;

[assembly: AssemblyTitle("Focal Point Cropping")]
[assembly: AssemblyDescription("Focal point based image cropping for EPiServer using ImageResizing.NET")]
[assembly: AssemblyCompany("Creuna AB")]
[assembly: AssemblyCopyright("Copyright 2016 Creuna AB")]
[assembly: AssemblyProduct("ImageResizer.Plugins.EPiFocalPoint")]
[assembly: ComVisible(false)]
[assembly: InternalsVisibleTo("ImageResizer.Plugins.EPiFocalPoint.Tests")]
[assembly: Guid("4206e11f-3084-48b7-9ec3-203d2e5edbb2")]
[assembly: AssemblyVersion("1.3.2.0")]
[assembly: AssemblyFileVersion("1.3.2.0")]
[assembly: PlugInSummary(MoreInfoUrl = "<A href='https://github.com/CreunaAB/EPiFocalPoint'>GitHub Repository</A>", License = LicensingMode.Freeware)]
#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif