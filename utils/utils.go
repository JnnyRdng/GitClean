package utils

import (
	t "gitclean/types"
)

func Filter[T any](ss []T, test func(T) bool) (ret []T) {
	for _, s := range ss {
		if test(s) {
			ret = append(ret, s)
		}
	}
	return
}

type Paginatable interface {
	string | t.ProcessedBranch
}

type Page[T Paginatable] struct {
	pages      [][]T
	PageSize   int
	TotalPages int
}

func Paginate[T Paginatable](items []T, pageSize int) Page[T] {
	if pageSize <= 0 {
		panic("pageSize must be greater than 0")
	}

	var pages [][]T
	for i := 0; i < len(items); i += pageSize {
		end := i + pageSize
		if end > len(items) {
			end = len(items)
		}
		pages = append(pages, items[i:end])
	}
	totalPages := (len(items) + pageSize - 1) / pageSize
	var zero T
	for i := len(pages[totalPages-1]); i < pageSize; i++ {
		pages[totalPages-1] = append(pages[totalPages-1], zero)
	}
	return Page[T]{pages, pageSize, totalPages}
}

func (p Page[T]) GetIndex(currentPage int, i int) int {
	return (currentPage * p.PageSize) + i
}

func (p Page[T]) GetPageForIndex(i int) (int, []T) {
	page := i / p.PageSize
	return page, p.pages[page]
}
