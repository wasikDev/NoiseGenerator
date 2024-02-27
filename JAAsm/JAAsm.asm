
.code
clamp_color_value_below proc
  ; Wczytaj warto�� z wska�nika
    mov eax, [rcx]      ; Wczytaj warto�� z adresu pami�ci w RCX do EAX

    ; Por�wnaj warto�� z 0
    cmp eax, 0
    jge check_upper_limit   ; Skocz do check_upper_limit, je�li warto�� jest wi�ksza lub r�wna 0

    ; Je�li warto�� jest mniejsza ni� 0, ustaw j� na 0
    mov eax, 0
    mov [rcx], eax      ; Zapisz zmodyfikowan� warto�� z powrotem do adresu pami�ci
    ret

check_upper_limit:
    ; Por�wnaj warto�� z 255
    cmp eax, 255
    jle finish           ; Skocz do finish, je�li warto�� jest mniejsza lub r�wna 255

    ; Je�li warto�� jest wi�ksza ni� 255, ustaw j� na 255
    mov eax, 255
    mov [rcx], eax      ; Zapisz zmodyfikowan� warto�� z powrotem do adresu pami�ci

finish:
    ret
clamp_color_value_below endp


add_noise_asm proc
    ; Sprawd�, czy indeks nie wykracza poza d�ugo�� tablicy
    cmp     r8, rdx
    jae     finish

    ; Zmie� kolor piksela na indeksie noisePixelIndex
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
    mov     rbx, rax        ; Przenie� warto�� do RBX dla dalszych operacji

    ; Generowanie pseudolosowej liczby
    xor     rdx, rdx        ; Wyczy�� RDX na potrzeby dzielenia
    mov     rax, rbx        ; U�yj warto�ci z RBX
    div     rcx             ; Dziel RAX przez d�ugo�� tablicy, wynik w RAX, reszta w RDX
    mov     eax, edx        ; U�yj reszty z dzielenia (RDX) jako indeksu
    ret

generate_random_index endp

; void add_noise_to_pixel(int* pixelData, int noiseValue, int index);
; x64 calling convention
; arg 0 - RCX - pointer to pixelData array
; arg 1 - RDX - noise value
; arg 2 - R8  - index of the pixel

add_noise_to_pixel proc
    movd    xmm0, edx        ; Za�aduj warto�� szumu do xmm0
    

    mov     eax, [rcx + r8*4] ; Za�aduj piksel do rejestru og�lnego przeznaczenia EAX
    movd    xmm1, eax         ; Przenie� dane z EAX do xmm1
    punpcklbw xmm1, xmm6     ; Konwersja piksela do postaci 16-bitowej

    paddw   xmm1, xmm0       ; Dodaj warto�� szumu do piksela
    packuswb xmm1, xmm1      ; Spakuj z powrotem do postaci 8-bitowej

  

    movd    rax, xmm1        ; Przenie� dane z xmm1 do rejestru og�lnego przeznaczenia RAX
    mov     [rcx + r8*4], eax ; Zapisz zmodyfikowany piksel z RAX z powrotem do tablicy
    ret

add_noise_to_pixel endp











end







