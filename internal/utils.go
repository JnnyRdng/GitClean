package internal

func Filter[T any](ss []T, test func(T) bool) (ret []T) {
	for _, s := range ss {
		if test(s) {
			ret = append(ret, s)
		}
	}
	return
}

type Page struct {
	pages      [][]string
	PageSize   int
	TotalPages int
}

func Paginate(items []string, pageSize int) Page {
	if pageSize <= 0 {
		panic("pageSize must be greater than 0")
	}

	var pages [][]string
	for i := 0; i < len(items); i += pageSize {
		end := i + pageSize
		if end > len(items) {
			end = len(items)
		}
		pages = append(pages, items[i:end])
	}
	totalPages := (len(items) + pageSize - 1) / pageSize
	for i := len(pages[totalPages-1]); i < pageSize; i++ {
		pages[totalPages-1] = append(pages[totalPages-1], "")
	}
	return Page{pages, pageSize, totalPages}
}

func (p Page) GetIndex(currentPage int, i int) int {
	return (currentPage * p.PageSize) + i
}

func (p Page) GetPageForIndex(i int) (int, []string) {
	page := i / p.PageSize
	return page, p.pages[page]
}
