			dir("/TestResults") {}
			
			// Currently we need to execute the tests like this, because the pipeline docker plugin being aware of DIND, and attempting to map
			// the volume to the bare metal host
			
			//Do not modify this unless you know the difference between ' and " in bash
			// (https://www.gnu.org/software/bash/manual/html_node/Quoting.html#Quoting) see (https://gist.github.com/fuhbar/d00d11297a48b892684da34360e4135a) for Jenkinsfile 
			// specific escaping examples. One day we might be able to test solutions (and have the results go to a specific directory) rather than specific projects, negating the need for such a complex command.
			def testCommand = $/docker run -v ${env.WORKSPACE}/TestResults:/TestResults --env-file ${servicePath}//build//unit_tests.env ${build_container.id} bash -c 'cd /build//$+servicePath+$/ && ls tests/*/*/*.csproj | xargs -I@ -t dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:CoverletOutput="/TestResults/TestCoverage/" --test-adapter-path:. --logger:"nunit;LogFilePath=/TestResults/@.xml" @'/$
			
			//Run the test command generated above
			sh(script: testCommand)