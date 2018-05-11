using System;
using System.Collections.Generic;
using System.Text;
using VSS.TRex.TAGFiles.Classes.Validator;

namespace VSS.TRex.TAGFiles.Classes
{
    public static class TagfileReposity
    {
        /// <summary>
        /// Achives tagfile incase of reprocessing
        /// </summary>
        /// <param name="tagDetail"></param>
        /// <returns></returns>
        public static bool ArchiveTagfile(TagfileDetail tagDetail)
        {
            // todo Should be archived to a common location. To preserve state I sugest saving all details as a json file which includes submission state and binary content. 
            // previously we used xml files but no reason it could not be kepted all together
            // you could also have routines like get alltagfiles for site x
            return true;
        }

        public static bool MoveToUnableToProcess(TagfileDetail tagDetail)
        {
            // todo Should be moved to a common location. To preserve state I sugest saving all details as a json file which includes the state and binary content. 
            return true;
        }


        public static bool RemoveTagfileArchive(TagfileDetail tagDetail)
        {
            // todo 
            return true;
        }


    }
}
