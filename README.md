iCloudLib
==========
iCloudLib is an open source C# library that allows one to access various web services provided through Apple's iCloud servers. This library utilizes the Json.NET library (http://json.codeplex.com/) to construct requests and parse responses from the servers.


FUNCTIONS
=========
* Keeps authentication alive to iCloud Servers.
* Structures describing the account, services accessible and other miscellaneous information.
* Contacts Services
* Find My iPhone / Device Services (LostMode+WipeDevice=TODO) 

NOTES
=====
* This is a work in progress, and not a project I gave or am going to give much focus to either.
* It has been made open source so that others can understand how iCloud services work, and how they could expand on them if desired.
* There is a test app included which simply shows you device and contact information, but has snippets showing you how to create, edit, or remove contacts, as well as track device locations/play sounds to them.
* If you wish to expand on this, simply load up a browser, go to iCloud.com, and watch the traffic between your client and the server, and use this library as a guide for how to communicate and analyze the information.
* This has been released under GNU GENERAL PUBLIC LICENSE Version 3, so please obey that. A license should have been provided. If not, it should remain at the top of the iCloud.cs file in the iCloudLib.