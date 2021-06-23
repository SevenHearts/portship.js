#include <stdlib.h>
#include <stdio.h>
#include <errno.h>
#include <string.h>

#define ROSE_MIN(x, y) ((x)<(y)?(x):(y))

int main(int argc, char *argv[]) {
	if (argc != 5) return 2;

	int status = 1;

	const char *archive = argv[1];
	unsigned long long offset = strtoull(argv[2], NULL, 10);
	unsigned long long length = strtoull(argv[3], NULL, 10);
	const char *outFile = argv[4];

	FILE *fd = fopen(archive, "rb");
	if (fd == NULL) {
		fprintf(stderr, "error: fopen(): %s: %s\n", strerror(errno), archive);
		goto exit;
	}

	if (fseek(fd, offset, SEEK_SET) != 0) {
		fprintf(stderr, "error: fseek(): %s: %s\n", strerror(errno), archive);
		goto exit_close;
	}

	unsigned long int curPos = ftell(fd);
	if (curPos == (unsigned long int)(-1L)) {
		fprintf(stderr, "error: ftell(): %s: %s\n", strerror(errno), archive);
		goto exit_close;
	}

	if (curPos != offset) {
		fprintf(stderr, "error: offset exceeds file length: %llu: %s\n", offset, archive);
		goto exit_close;
	}

	FILE *outfd = fopen(outFile, "wb");
	if (outfd == NULL) {
		fprintf(stderr, "error: fopen(): %s: %s\n", strerror(errno), outFile);
		goto exit_close;
	}

	char buffer[1024*4];
	size_t nxferd;
	size_t to_read;

	while (length > 0) {
		to_read = ROSE_MIN(sizeof(buffer), length);
		length -= to_read;

		nxferd = fread(buffer, 1, to_read, fd);

		if (nxferd < to_read) {
			if (ferror(fd)) {
				fprintf(stderr, "error: fread(): %s: %s\n", strerror(errno), archive);
			} else if (feof(fd)) {
				fprintf(
					stderr,
					"error: fread(): unexpectedly hit EOF (are length/offset correct?): "
					"%s (length = %s, tried to read %zu bytes)\n",
					archive,
					argv[3],
					to_read
				);
			} else {
				fprintf(stderr, "error: fread(): unknown error: %s\n", archive);
			}

			goto exit_close_both;
		}

		nxferd = fwrite(buffer, 1, to_read, outfd);

		if (nxferd < to_read) {
			if (ferror(outfd)) {
				fprintf(stderr, "error: fwrite(): %s: %s\n", strerror(errno), outFile);
			} else {
				fprintf(stderr, "error: fwrite(): unknown error: %s\n", outFile);
			}

			goto exit_close_both;
		}
	}

	status = 0;

exit_close_both:
	fclose(outfd);
exit_close:
	fclose(fd);
exit:
	return status;
}
