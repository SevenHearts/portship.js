meta:
  id: rose_log
  file-extension: lod
  endian: le
seq:
  - id: name
    type: strz
    encoding: ascii
  - id: lod
    type: u4
    repeat: expr
    repeat-expr: 0x3c1
