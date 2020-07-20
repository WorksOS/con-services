# Requirements #

## Public/Private Key Pairs ##
You must ensure your private key is present in the `/Setup` folder before running the synchronisation script. It must be labeled `id_rsa`.

This private key is part of the pair where the public key has been uploaded to Artifactory (https://artifactory.trimble.tools/) and Bitbucket (https://bitbucket.trimble.tools/projects/TGL).

In both platforms your public SSH key can be loaded under your profile/account page.

If the public key has not been imported into both Artifactory and Bitbucket.Trimble.Tools the repository will not be cloned.

## Running the Script ##
If the setup is correct you will see the following logging during execution of the synchronisation script:
```
...
Step 9/10 : RUN git clone ssh://git@bitbucket.trimble.tools/tgl/tgl_geodata.git
 ---> Running in 5bba112c80a4
Cloning into 'tgl_geodata'...
Warning: Permanently added the RSA host key for IP address '34.212.206.231' to the list of known hosts.
Downloading 27alaska.mrp (811 B)
Downloading 27canada.mrp (1.38 KB)
Downloading 27carib.mrp (703 B)5)
...
```

## AWS Login ##
You must be logged in to AWS (Okta login), in order for the sync process to push to S3.

## Remarks ##
Do not update the `git-lfs` package. It's the most recent version available that Artifactory will still work with (Artifactory uses deprecated Git APIs and cannot use `git-lfs` > v1.5.6).