meta:
  id: rose_mov
  file-extension: mov
  endian: le
seq:
  - id: width
    type: u4
  - id: height
    type: u4
  - id: is_walkable
    type: u1
    repeat: expr
    repeat-expr: width * height
