int match_otp(int argc, const char** argv);
int get_otp(int argc, const char** argv);
int calculate_otp(const char *secret, long tm, int digits);


void encode_base32(int argc, const char** argv);
unsigned char* get_secret(int argc, const char** argv);
long get_time(int argc, const char** argv);
int get_digits(int argc, const char** argv);
int get_window(int argc, const char** argv);
int get_password(int argc, const char** argv);


void print_hex(const void *data, int start, int count);
const char* get_arg(int argc, const char **argv, const char *param_name);

void b64_decode(const char *b64src, char *clrdst);
void b64_encode(const char *clrstr, char *b64dst);


