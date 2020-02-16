Please follow these steps to add or update NHSvr resources:

1. Changes to resources must be made in the NHSvr.tdb file, in the first instance. This is 
the master source file for resource strings.
Note: 
a. Do not edit the .tdb file in a text editor, but instead use the TransIt tool 
(as there are implications on translations related to existing strings being modified). 
Refer to the bottom section of this file for steps on how to use the TransIt tool.
b. It is important you do not edit the english resx file directly in either a text editor
or in VS2008 as this will cause problems (see below).

2. Save the changes to the tdb.

3. Check out the following folder to allow the batch files to edit them properly:
	a.the Resource folder (parent folder)...the folder needs to have write permissions.  This will check out everything you need.

4. Run the TdbToResX tool from the command line, as follows:
  tdbtoresx.exe /tdb=NHSvr.
Check for errors. Correct any errors. 
Repeat until the tool runs without errors.

5. Build the Resources project. 

6. The english resx file, has a C# class, which is a convenient way to access the resource strings
programmatically. This designer file, called NHSvr.Designer.cs, is updated by TdbToResx using the ResGen utility.



---------------------------------------
[Using TransIt to edit .tdb files]

1. Run TransIt.exe
2. Go to File/Open... and open up NHSvr.tdb (ensure NHSvr.tdb is checked out before this step).
3. To add a string:
     - Go to Edit/Add Token
     - Key in a unique Token name
     - Key in the English string you wish to add
     - Add a comment that to provide context to the translator as to the usage of the string (e.g. whether 'lock' is a verb, or a noun, whether there are string length restrictions, etc.)

   To edit an existing string:
     - Navigate to the string via the list in the left column (or use Ctrl+F)
     - Select string from the list; edit string in the 'English' edit box field on the right hand side

4. Save before exiting TransIt.


--------------------------------------------

If there is a problem when running ResGen then run it by itself in a command window to see what the error is:

ResGen NHSvr.resx /str:cs,VSS.Hosted.VLCommon.Resources /publicClass