#include "format/rose_zms.h"

#include <iostream>
#include <fstream>

template <typename T>
static void throw_on_error(T &stream) {
	stream.exceptions(std::ifstream::failbit | std::ifstream::badbit);
}

int main(int argc, char *argv[]) {
	if (argc != 3) {
		std::cerr << "usage: " << argv[0] << " <input.zms> <output.obj>" << std::endl;
		return 2;
	}

	std::ifstream input;
	throw_on_error(input);
	input.open(argv[1], std::ifstream::binary);

	kaitai::kstream ks(&input);

	rose_zms_t zms = rose_zms_t(&ks);
	switch (zms.version()) {
		case rose_zms_t::zms_version_t::ZMS_VERSION_V7:
		case rose_zms_t::zms_version_t::ZMS_VERSION_V8:
			break;
		case rose_zms_t::zms_version_t::ZMS_VERSION_V6:
			break;
		default:
			std::cerr << "error: unsupported ZMS version" << std::endl;
			return 1;
	}

	return 0;
}
