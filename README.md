# Hosta

A secure, private, peer-to-peer social media.

The interesting parts are in the Hosta folder/project - it is the central library I will use throughout.

 
.github/workflows:

 - Automatically runs the unit tests on new commit

 
.vscode:

 - vscode metadata

 
CryptoTest:

 - Used to check stuff as I write without formally writing a test

 
Hosta:

 - The core library
 - Crypto - Manages all non-implementation specific cryptography
 - Exceptions - all the custom exceptions to be used
 - Net - Allows the abstraction of inter-device communications with layers. Each layer handles a different part of the process, with the lowest down layer converting to a stream (SocketStream for production, LocalStream for easier testing). All interactions are asynchronous/task-based, to allow for better scaling.
	Tools - random classes that I find useful throughout the project

 
HostaTests:

 - VS integrated unit testing for the Hosta library

 
Model (will be renamed to storage):

 - Provides a controlled interface to the underlying database.

 
UI (will be renamed to View, might be platform agnostic in the future):

 - Displays information returned from the yet to be created Controller.
