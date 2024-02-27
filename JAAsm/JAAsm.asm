
.code
clamp_color_value_below proc
  ; Wczytaj wartoœæ z wskaŸnika
    mov eax, [rcx]      ; Wczytaj wartoœæ z adresu pamiêci w RCX do EAX

    ; Porównaj wartoœæ z 0
    cmp eax, 0
    jge check_upper_limit   ; Skocz do check_upper_limit, jeœli wartoœæ jest wiêksza lub równa 0

    ; Jeœli wartoœæ jest mniejsza ni¿ 0, ustaw j¹ na 0
    mov eax, 0
    mov [rcx], eax      ; Zapisz zmodyfikowan¹ wartoœæ z powrotem do adresu pamiêci
    ret

check_upper_limit:
    ; Porównaj wartoœæ z 255
    cmp eax, 255
    jle finish           ; Skocz do finish, jeœli wartoœæ jest mniejsza lub równa 255

    ; Jeœli wartoœæ jest wiêksza ni¿ 255, ustaw j¹ na 255
    mov eax, 255
    mov [rcx], eax      ; Zapisz zmodyfikowan¹ wartoœæ z powrotem do adresu pamiêci

finish:
    ret
clamp_color_value_below endp


add_noise_asm proc
    ; SprawdŸ, czy indeks nie wykracza poza d³ugoœæ tablicy
    cmp     r8, rdx
    jae     finish

    ; Zmieñ kolor piksela na indeksie noisePixelIndex
    mov     [rcx + r8 * 4], r9

finish:
    ret

; int generate_random_index(int arrayLength);
; x64 calling convention
; arg 0 - RCX - length of the array
add_noise_asm endp
generate_random_index proc
    ; Inicjalizacja generatora liczb losowych
    rdtsc                   ; Odczytaj licznik czasu procesora
    mov     rbx, rax        ; Przenieœ wartoœæ do RBX dla dalszych operacji

    ; Generowanie pseudolosowej liczby
    xor     rdx, rdx        ; Wyczyœæ RDX na potrzeby dzielenia
    mov     rax, rbx        ; U¿yj wartoœci z RBX
    div     rcx             ; Dziel RAX przez d³ugoœæ tablicy, wynik w RAX, reszta w RDX
    mov     eax, edx        ; U¿yj reszty z dzielenia (RDX) jako indeksu
    ret

generate_random_index endp

; void add_noise_to_pixel(int* pixelData, int noiseValue, int index);
; x64 calling convention
; arg 0 - RCX - pointer to pixelData array
; arg 1 - RDX - noise value
; arg 2 - R8  - index of the pixel

add_noise_to_pixel proc
    movd    xmm0, edx        ; Za³aduj wartoœæ szumu do xmm0
    

    mov     eax, [rcx + r8*4] ; Za³aduj piksel do rejestru ogólnego przeznaczenia EAX
    movd    xmm1, eax         ; Przenieœ dane z EAX do xmm1
    punpcklbw xmm1, xmm6     ; Konwersja piksela do postaci 16-bitowej

    paddw   xmm1, xmm0       ; Dodaj wartoœæ szumu do piksela
    packuswb xmm1, xmm1      ; Spakuj z powrotem do postaci 8-bitowej

  

    movd    rax, xmm1        ; Przenieœ dane z xmm1 do rejestru ogólnego przeznaczenia RAX
    mov     [rcx + r8*4], eax ; Zapisz zmodyfikowany piksel z RAX z powrotem do tablicy
    ret

add_noise_to_pixel endp











end







