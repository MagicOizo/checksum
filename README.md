# checksum
Windows command line tool to generate hashes of strings or files. Supported hashes MD5, SHA1, SHA256, SHA384, SHA512.

Easy to use and compareable to the well known linux checksum tools.

The tool has a build in help to find out how to use it. Just type "checksum -?".

Easy to gernerate the hash of a file by using 'checkum -f <filename> -h sha256' to output the SHA256 hash of a file.
Also easy to generate the hash of a string by using 'checksum -s "Hello World" -h sha256' to output the SHA256 of "Hello World".

Using the .Net-Crypto API.
