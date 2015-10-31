Tera Connection Log Format
==========================

This is a file format to store a Tera connection log. The connection is decrypted and split into individual messages.

The log contains a sequence of blocks. Each block consists of a one byte Block Type and a variable length payload.
Readers should ignore blocks whose type they do not recognize.

Block layout
------------

    1 byte         Block Type
    2 bytes        Size (little endian)
    Size-2 bytes   Payload

The size field includes the size of itself, but not of the block id.

Block Types
-----------

    MagicBytes = 1
    Start      = 2
    Timestamp  = 3
    Client     = 4
    Server     = 5
    Region     = 6

Magic Bytes block
-----------------

This is the first block of the file and contains the ASCII string "TeraConnectionLog".

Start block
-----------

Separates header blocks from the actual connection log. It has no payload. Blocks that occur before it are called *header block*s. Blocks that occur after it are called *body block*s.

Timestamp block
---------------

This is a *body block*. The payload is a variable length, little endian, integer. It contains the number of milliseconds since Jan 1st, 2015 in UTC.

Its time is used for all following events, until the time is updated by another Timestamp block. Timestamps should increase monotonically.

There should be a *Timestamp block* before the first event block, so each event has a well defined time.

Server block
------------

This is a *body block*. Represents a Tera message sent from the server to the client.

Both size and payload of this block correspond directly to how Tera encodes a message.

The payload starts with a 16 bit OpCode in little endian format, the remainder is OpCode specific data.

Client block
------------

This is a *body block*. Represents a Tera message sent from the client to the server.

Otherwise identical to *Server block*s.

Region block
------------

This is a *header block*. The payload is an ASCII string identifying the region the log was recorded in. Currently uses "EU", "NA", "RU" and "KR".