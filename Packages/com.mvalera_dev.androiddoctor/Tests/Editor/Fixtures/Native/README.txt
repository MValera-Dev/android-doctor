int android_doctor_test_answer(void) { return 42; }
// built with the NDK r27c that ships with Unity 6000.6.4f1:
// aarch64-linux-android26-clang -shared -o x.so test.c -Wl,-z,max-page-size=4096 (or 16384)
// armv7a-linux-androideabi26-clang -shared -o x.so test.c
