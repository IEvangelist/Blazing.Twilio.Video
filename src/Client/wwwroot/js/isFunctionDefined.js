export function isFunctionDefined(instance, member) {
    return !!instance && typeof instance[member] === 'function';
}
