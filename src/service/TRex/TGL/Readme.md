# Requirements #

## Public/Private Key Pairs ##
You must ensure your private key is present in the `/Setup` folder before running the synchronisation script. It must be labeled `id_rsa`.

This private key is part of the pair where the public key has been uploaded to Artifactory (https://artifactory.trimble.tools/) and Bitbucket (https://bitbucket.trimble.tools/projects/TGL).

In both platforms your public SSH key can be loaded under your profile/account page.

If the public key has not been imported into both Artifactory and Bitbucket.Trimble.Tools the repository will not be cloned.
<br><br>

## AWS Login ##
You must be logged in to AWS in order for the sync process to push to S3.

## Remarks ##
Do not update the `git-lfs` package. It's the most recent version available that Artifactory will still work with (Artifactory uses deprecated Git APIs and cannot use `git-lfs` > v1.5.6).